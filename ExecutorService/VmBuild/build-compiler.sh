#!/bin/bash

MICRONAUT_JAR_PATH="${1:-$(find / -name "compiler-0.1-all.jar" 2>/dev/null | head -1)}"

STARTING_DIR=$(pwd)
cd /tmp

dd if=/dev/zero of=compiler-fs.ext4 bs=1M count=0 seek=512
mkfs.ext4 compiler-fs.ext4

mkdir -p /tmp/compiler-rootfs

sudo mount pull compiler-fs.ext4 /tmp/compiler-rootfs

cd /tmp/compiler-rootfs

curl -O https://dl-cdn.alpinelinux.org/alpine/v3.21/releases/x86_64/alpine-minirootfs-3.21.3-x86_64.tar.gz
tar -xpf alpine-minirootfs-3.21.3-x86_64.tar.gz
rm -rf alpine-minirootfs-3.21.3-x86_64.tar.gz

cat > "/tmp/compiler-rootfs/etc/resolv.conf" << EOF
nameserver 127.0.0.1
nameserver 1.1.1.1
nameserver 8.8.8.8
EOF

cat > "/tmp/compiler-rootfs/etc/apk/repositories" << EOF
http://dl-cdn.alpinelinux.org/alpine/v3.21/main
http://dl-cdn.alpinelinux.org/alpine/v3.21/community
EOF

mkdir -p /tmp/compiler-rootfs/app

cp $MICRONAUT_JAR_PATH /tmp/compiler-rootfs/app/compiler.jar

mkdir -p /tmp/compiler-rootfs/app/lib

curl -o "/tmp/compiler-rootfs/app/lib/gson-2.13.1.jar" https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar

chmod a-w "/tmp/compiler-rootfs/app/lib/gson-2.13.1.jar"
chmod a+r "/tmp/compiler-rootfs/app/lib/gson-2.13.1.jar"

mkdir -p /tmp/compiler-rootfs/app/scripts

cat > "/tmp/compiler-rootfs/app/scripts/compiler-src.sh" << 'EOF'
#!/bin/sh
CLASS_NAME="$1"
CODE_B64="$2"
EXEC_ID="$3"

mkdir -p "/app/client-src/$EXEC_ID"
mkdir -p "/app/error-log/$EXEC_ID"
echo "$CODE_B64" | base64 -d > "/app/client-src/$EXEC_ID/$CLASS_NAME.java"

javac -cp "/app/lib/gson-2.13.1.jar" -proc:none -d "/app/client-bytecode/$EXEC_ID" "/app/client-src/$EXEC_ID/$CLASS_NAME.java" 2>"/app/error-log/$EXEC_ID/err.log"
EOF


cat > "/tmp/compiler-rootfs/app/scripts/get-file-hash.sh" << 'EOF'
#!/bin/sh

FILE_PATH=$1

if [ -f $FILE_PATH ]; then
  sha256sum $FILE_PATH
else
  exit 1
fi
EOF


cat > "/tmp/compiler-rootfs/app/proxy.sh" << 'EOF'
#!/bin/sh

while ! nc -z 127.0.0.1 5137; do
  sleep 1s
done

socat VSOCK-LISTEN:5050,fork EXEC:"/bin/sh -c /app/process-input.sh" &

# TODO: hardcoded sleeps are highly problematic. Due to the nature of health-checks however 
# (we need to get file hashes before the machine is passed into an active pool). EDIT: No, machine is built base on static image. We can precompute hashes
# It is queried immediately which tends to cause race conditions.
sleep 5s

echo "READY" > /dev/ttyS0
EOF

cat > "/tmp/compiler-rootfs/app/RequestHandler.java" << 'EOF'
import com.google.gson.*;
import java.io.*;
import java.net.*;
import java.net.http.HttpClient;
import java.net.http.HttpRequest;
import java.net.http.HttpResponse;
import java.nio.charset.StandardCharsets;
import java.time.Duration;
import java.time.temporal.ChronoUnit;
import java.util.Base64;
import java.util.HashMap;
import java.util.Map;

class RequestData {
    String Endpoint = "health";
    Method Method = new Method();
    JsonObject Content;
    String Ctype = "application/json";
}

class Method {
    String Method = "GET";
}

public class RequestHandler {
    private static final Gson gson = new Gson();

    public static void main(String[] args) throws Exception {
        if (args.length != 1) return;
        String input = args[0];

        String jsonString = new String(Base64.getDecoder().decode(input), StandardCharsets.UTF_8);
        RequestData request = gson.fromJson(jsonString, RequestData.class);

        makeHttpRequest(request);
    }

    private static void makeHttpRequest(RequestData request) throws Exception {
        var builder = HttpRequest.newBuilder(new URI((String.format("http://127.0.0.1:5137/%s", request.Endpoint))))
                .timeout(Duration.of(10, ChronoUnit.SECONDS));

        addHeaders(builder, new HashMap<>());
        setMethod(builder, request);

        HttpResponse<String> response = HttpClient.newBuilder()
                .build()
                .send(builder.build(), HttpResponse.BodyHandlers.ofString());

        System.out.printf("%s %s%n", response.version(), response.statusCode());

        response.headers().map().forEach((key, value) -> System.out.printf("%s: %s\n", key, value));

        System.out.printf("\n%s\n", response.body());
    }

    private static void addHeaders(HttpRequest.Builder builder, Map<String, String> headers) {
        headers.forEach(builder::header);
    }

    private static void setMethod(HttpRequest.Builder builder, RequestData request) {
        String method = request.Method.Method.toUpperCase();
        if (method.equals("POST") && request.Content != null) {
            builder.POST(HttpRequest.BodyPublishers.ofString(gson.toJson(request.Content), StandardCharsets.UTF_8))
                    .header("Content-Type", request.Ctype);
        } else {
            builder.method(method, HttpRequest.BodyPublishers.noBody());
        }
    }
}
EOF

cat > "/tmp/compiler-rootfs/app/process-input.sh" << 'EOF'
#!/bin/sh

read_until_eot() {
  local input=""
  local char=""
  while IFS= read -r -n1 char; do
    if [ "$(printf '%d' "'$char")" = "4" ]; then
      break
    fi
    input="$input$char"
  done
  echo "$input"
}

while true; do
  payload=$(read_until_eot)
  [ -z "$payload" ] && continue
  
  response=$(cd /app && java -cp ".:lib/gson-2.13.1.jar" RequestHandler "$payload")
  
  printf "%s" "$response"
  printf '\004'
done
EOF

chmod +x /tmp/compiler-rootfs/app/proxy.sh
chmod +x /tmp/compiler-rootfs/app/process-input.sh
chmod +x /tmp/compiler-rootfs/app/scripts/compiler-src.sh
chmod +x /tmp/compiler-rootfs/app/scripts/get-file-hash.sh

mount -t proc proc proc/
mount -t sysfs sys sys/
mount -o bind /dev dev/
mount -o bind /dev/pts dev/pts/

chroot /tmp/compiler-rootfs /bin/sh << 'EOF'
apk update
apk add openjdk17-jdk coreutils openrc mdevd socat netcat-openbsd

echo 'ttyS0 root:root 660' > /etc/mdevd.conf

cat > "/etc/init.d/entrypoint" << 'INNER_EOF'
#!/sbin/openrc-run
description="main process to start micronaut http server"
command="/usr/bin/java"
command_args="-jar /app/compiler.jar"
command_user="root"
command_background=true
pidfile="/run/entrypoint.pid"
command_env="LD_LIBRARY_PATH=/usr/lib/jvm/java-17-openjdk/lib"

depend(){
    need localmount
    need mdevd
    after lo
    after net
}
INNER_EOF

cat > "/etc/init.d/proxy" << 'INNER_EOF'
#!/sbin/openrc-run

description="Proxy for bouncing vsock requests to http server"
command="/app/proxy.sh"
command_background=true
pidfile="/run/proxy.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
    after entrypoint
    after lo
}

start_post(){
    exit 0
}

INNER_EOF

cat > "/etc/init.d/lo" << 'INNER_EOF'
#!/sbin/openrc-run

depend() {
    need localmount
    before net
    before entrypoint
}

start() {
    ip link set lo up
    ip addr add 127.0.0.1/8 dev lo 2>/dev/null || true
}

INNER_EOF

chmod +x /etc/init.d/lo
rc-update add lo boot

chmod +x /etc/init.d/entrypoint
rc-update add entrypoint default

chmod +x /etc/init.d/proxy
rc-update add proxy default

javac -cp "/app/lib/gson-2.13.1.jar" /app/RequestHandler.java -d /app/
rm -rf /app/RequestHandler.java

EOF

cd ~/

umount /tmp/compiler-rootfs/dev/pts
umount /tmp/compiler-rootfs/dev
umount /tmp/compiler-rootfs/proc
umount /tmp/compiler-rootfs/sys
umount /tmp/compiler-rootfs

rm -rf /tmp/compiler-rootfs
mv /tmp/compiler-fs.ext4 $STARTING_DIR