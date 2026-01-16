import os
import socket
import json
import subprocess
import base64
import time
import logging

logging.basicConfig(
    level=logging.DEBUG,
    format='%(asctime)s - %(levelname)s - %(message)s',
    handlers=[
        logging.FileHandler('/dev/ttyS0', mode='a')
    ]
)
logger = logging.getLogger(__name__)

PORT = 5050

logger.info("Starting sandbox server...")

try:
    logger.debug(f"Creating VSOCK socket on port {PORT}")
    sock = socket.socket(socket.AF_VSOCK, socket.SOCK_STREAM)
    sock.bind((socket.VMADDR_CID_ANY, PORT))
    sock.listen(1)
    logger.info(f"Socket bound and listening on port {PORT}")
    with open('/dev/ttyS0', 'w') as f:
        f.write("READY")

except Exception as e:
    logger.error(f"Failed to create/bind socket: {e}", exc_info=True)
    raise

try:
    logger.info("Waiting for connection...")
    conn, addr = sock.accept()
    logger.info(f"Connection accepted from {addr}")
except Exception as e:
    logger.error(f"Failed to accept connection: {e}", exc_info=True)
    raise


def set_up_cgroup() -> None:
    logger.debug("Setting up cgroup...")
    try:
        with open("/sys/fs/cgroup/sandbox/cgroup.procs", "w") as f:
            f.write(str(os.getpid()))
        logger.info(f"Successfully added PID {os.getpid()} to cgroup")
    except Exception as e:
        logger.error(f"Failed to set up cgroup: {e}", exc_info=True)
        raise


def read_from_vsock(conn: socket.socket) -> bytes:
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


try:
    read_bytes = read_from_vsock(conn)
    logger.debug(f"Parsing JSON data ({len(read_bytes)} bytes)")
    
    json_data = json.loads(read_bytes.decode('utf-8'))
    entrypoint = json_data['Entrypoint']
    logger.info(f"Entrypoint: {entrypoint}")
    
    class_files = json_data.get('ClientSrc') or json_data.get('clientSrc') or json_data.get('client_src')
    logger.info(f"Received {len(class_files)} file(s) to write")
    
    for filename, file_contents in class_files.items():
        filepath = f'/sandbox/{filename}'
        logger.debug(f"Writing file: {filepath}")
        with open(filepath, 'wb') as f:
            java_code = base64.b64decode(file_contents)
            f.write(java_code)
        logger.debug(f"Written {len(java_code)} bytes to {filepath}")

    sandbox_contents = os.listdir('/sandbox')
    logger.info(f"Sandbox contents: {sandbox_contents}")

except Exception as e:
    logger.error(f"Failed processing request: {e}", exc_info=True)
    error_response = json.dumps({'err': f'failed processing request: {e}'}).encode('utf-8') + b'\x04'
    conn.sendall(error_response)
    conn.close()
    sock.close()
    raise

set_up_cgroup()

log_file = "/sandbox/time.log"
java_cmd = ["/usr/bin/time", "-v", "-o", log_file, "java", "-cp", "/sandbox:.:/sandbox/gson-2.13.1.jar", entrypoint]
logger.info(f"Executing command: {' '.join(java_cmd)}")

start_ns = time.time_ns()
logger.debug(f"Start time (ns): {start_ns}")

try:
    process = subprocess.Popen(
        java_cmd,
        stdout=subprocess.PIPE,
        stderr=subprocess.PIPE,
        text=True
    )
    logger.debug(f"Process started with PID: {process.pid}")
    
    stdout, stderr = process.communicate()
    end_ns = time.time_ns()
    
    duration_ms = (end_ns - start_ns) / 1_000_000
    logger.info(f"Process completed in {duration_ms:.2f}ms with exit code {process.returncode}")
    
    if stdout:
        logger.debug(f"stdout ({len(stdout)} chars): {stdout[:500]}{'...' if len(stdout) > 500 else ''}")
    if stderr:
        logger.warning(f"stderr ({len(stderr)} chars): {stderr[:500]}{'...' if len(stderr) > 500 else ''}")

except Exception as e:
    end_ns = time.time_ns()
    logger.error(f"Process execution failed: {e}", exc_info=True)
    stdout = ""
    stderr = str(e)
    process = type('obj', (object,), {'returncode': -1})()

max_mem_kb = 0
try:
    with open(log_file) as file:
        lines = [line.rstrip() for line in file if "Maximum resident set size" in line]
        if lines:
            max_mem_kb = int(lines[0].split()[-1])
            logger.info(f"Maximum memory usage: {max_mem_kb} KB ({max_mem_kb / 1024:.2f} MB)")
        else:
            logger.warning("Could not find memory usage in time log")
except Exception as e:
    logger.error(f"Failed to parse time log: {e}", exc_info=True)

output = {
    'out': stdout,
    'err': stderr,
    'exitCode': f"{process.returncode}",
    'startNs': f"{start_ns}",
    'endNs': f"{end_ns}",
    'maxMemoryKb': f"{max_mem_kb}"
}
logger.debug(f"Output payload: exitCode={output['exitCode']}, maxMemoryKb={output['maxMemoryKb']}")

try:
    response = json.dumps(output).encode('utf-8') + b'\x04'
    logger.debug(f"Sending response ({len(response)} bytes)")
except Exception as e:
    logger.error(f"Failed serializing output: {e}", exc_info=True)
    response = json.dumps({'err': f'failed serializing output: {e}'}).encode('utf-8') + b'\x04'

try:
    conn.sendall(response)
    logger.info("Response sent successfully")
except Exception as e: 
    logger.error(f"failed sending response: {e}")

conn.close()
sock.close()
logger.info("Connections closed, sandbox server shutting down")