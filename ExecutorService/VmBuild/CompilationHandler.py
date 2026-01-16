import os, socket, json, subprocess, base64, time, logging, hashlib

logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/dev/ttyS0', mode='a')
    ]
)
logger = logging.getLogger(__name__)

PORT = 5050

class Handler:
    def __init__(self):
        logger.info("Initializing compiler handler...")
        self.sock = socket.socket(socket.AF_VSOCK, socket.SOCK_STREAM)
        self._request_count = 0

    def handle(self):
        logger.debug(f"Binding to VSOCK port {PORT}")
        self.sock.bind((socket.VMADDR_CID_ANY, PORT))
        self.sock.listen(1)
        logger.info(f"Socket bound and listening on port {PORT}")

        with open('/dev/ttyS0', 'w') as f:
            f.write("READY")

        try:
            while True:
                logger.debug("Waiting for connection...")
                conn, addr = self.sock.accept()
                self._request_count += 1
                logger.info(f"Connection accepted (request #{self._request_count}) from {addr}")

                try:
                    req = self._read_from_vsock(conn)
                    json_data = json.loads(req.decode('utf-8'))
                    logger.debug(f"Parsed request JSON, keys: {list(json_data.keys())}")
                    result = self._process_request(json_data)
                    conn.sendall(result)
                    logger.info(f"Request #{self._request_count} completed successfully")
                except json.JSONDecodeError as e:
                    logger.error(f"JSON parse error: {e}", exc_info=True)
                    err = json.dumps({"$type": "err", "body": f"JSON parse error: {str(e)}"}).encode() + b'\x04'
                    conn.sendall(err)
                except Exception as e:
                    logger.error(f"Request processing error: {e}", exc_info=True)
                    err = json.dumps({"$type": "err", "body": str(e)}).encode() + b'\x04'
                    conn.sendall(err)
                finally:
                    conn.close()
                    logger.debug("Connection closed")
        finally:
            self.sock.close()
            logger.info("Socket closed, handler shutting down")

    def _read_from_vsock(self, conn: socket.socket) -> bytes:
        logger.debug("Reading data from VSOCK...")
        chunks = []
        total_bytes = 0
        chunk_count = 0

        while True:
            chunk = conn.recv(4096)

            chunk_count += 1
            if not chunk:
                logger.debug(f"Received empty chunk, ending read after {chunk_count} chunks")
                break

            total_bytes += len(chunk)
            logger.debug(f"Received chunk {chunk_count}: {len(chunk)} bytes (total: {total_bytes})")

            if b'\x04' in chunk:
                chunks.append(chunk.split(b'\x04')[0])
                logger.debug("Found EOT marker, ending read")
                break
            chunks.append(chunk)

        result = b''.join(chunks)
        logger.info(f"Read complete: {total_bytes} bytes in {chunk_count} chunks")
        return result

    def _process_request(self, request: dict) -> bytes:
        job_type = request.get("$type")
        
        if job_type == "health":
            return self._process_health_check(request)
        
        return self._process_compilation(request)
        
    
    def _process_health_check(self, request: dict) -> bytes:
        files_to_check = request.get("FilesToCheck") or request.get("filesToCheck") or request.get("files_to_check")
        hashes = {}
        for file in files_to_check:
            logger.info(f"extracting hashes for: {file}")
            with open(file, 'rb') as f:
                hashes[file] = hashlib.sha256(f.read()).hexdigest()
        return json.dumps({ "fileHashes": hashes }).encode() + b'\x04'
    
    def _process_compilation(self, request: dict) -> bytes:
        job_id = request.get("jobId") or request.get("JobId") or request.get("job_id")
        client_src = request.get("clientSrc") or request.get("ClientSrc") or request.get("SrcFiles") or request.get("srcFiles")

        logger.info(f"Processing job: {job_id}")

        if not job_id:
            logger.warning("Request missing job_id")
            return json.dumps({"$type": "err", "body": "missing job_id"}).encode() + b'\x04'

        if not client_src:
            logger.warning(f"Job {job_id}: missing source files")
            return json.dumps({"$type": "err", "jobId": job_id, "body": "missing source files"}).encode() + b'\x04'

        src_dir = f"/app/client-src/{job_id}"
        bytecode_dir = f"/app/client-bytecode/{job_id}"

        os.makedirs(src_dir, exist_ok=True)
        os.makedirs(bytecode_dir, exist_ok=True)
        logger.debug(f"Created directories: {src_dir}, {bytecode_dir}")

        source_files = []
        for fname, contents in client_src.items():
            filepath = f"{src_dir}/{fname}.java"
            with open(filepath, "wb") as f:
                decoded = base64.b64decode(contents)
                f.write(decoded)
            logger.debug(f"Wrote source file: {filepath} ({len(decoded)} bytes)")
            source_files.append(filepath)

        logger.info(f"Job {job_id}: Compiling {len(source_files)} source file(s)")

        javac_cmd = [
                        "javac",
                        "-cp", "/app/lib/gson-2.13.1.jar",
                        "-proc:none",
                        "-d", bytecode_dir
                    ] + source_files

        logger.debug(f"Running: {' '.join(javac_cmd)}")

        start_ns = time.time_ns()
        process = subprocess.Popen(
            javac_cmd,
            stdout=subprocess.PIPE,
            stderr=subprocess.PIPE
        )
        _, stderr = process.communicate()
        end_ns = time.time_ns()

        duration_ms = (end_ns - start_ns) / 1_000_000
        logger.info(f"Job {job_id}: Compilation finished in {duration_ms:.2f}ms with exit code {process.returncode}")

        if process.returncode != 0:
            error_msg = stderr.decode('utf-8', errors='replace')
            logger.warning(f"Job {job_id}: Compilation failed - {error_msg[:200]}")
            response = {
                "$type": "err",
                "jobId": job_id,
                "body": error_msg
            }
        else:
            class_files = {}

            try:
                bytecode_contents = os.listdir(bytecode_dir)
                logger.debug(f"Job {job_id}: Bytecode dir contents: {bytecode_contents}")

                for f in bytecode_contents:
                    if f.endswith(".class"):
                        path = os.path.join(bytecode_dir, f)
                        with open(path, "rb") as fp:
                            content = fp.read()
                            class_files[f] = base64.b64encode(content).decode()
                        logger.debug(f"Read class file: {f} ({len(content)} bytes)")

            except Exception as e:
                logger.error(f"Job {job_id}: Error reading class files: {e}", exc_info=True)
                return json.dumps({"$type": "err", "jobId": job_id, "body": f"Error reading class files: {e}"}).encode() + b'\x04'

            logger.info(f"Job {job_id}: Successfully compiled {len(class_files)} class file(s)")
            response = {
                "$type": "ok",
                "jobId": job_id,
                "body": class_files
            }

        result = json.dumps(response).encode() + b'\x04'
        logger.debug(f"Job {job_id}: Sending response ({len(result)} bytes)")
        return result

if __name__ == "__main__":
    Handler().handle()