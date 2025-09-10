#!/bin/bash

MICRONAUT_JAR_PATH="${1:-$(find / -name "compiler-0.1-all.jar" 2>/dev/null | head -1)}"

STARTING_DIR=$(pwd)
cd /tmp

dd if=/dev/zero of=compiler-fs.ext4 bs=1M count=0 seek=512
mkfs.ext4 compiler-fs.ext4

mkdir -p /tmp/compiler-rootfs

sudo mount -o loop compiler-fs.ext4 /tmp/compiler-rootfs

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

echo $CLASS_NAME
echo $CODE_B64
echo $EXEC_ID

mkdir -p "/app/client-src/$EXEC_ID"
mkdir -p "/app/error-log/$EXEC_ID"
echo "$CODE_B64" | base64 -d > "/app/client-src/$EXEC_ID/$CLASS_NAME.java"

javac -cp "/app/lib/gson-2.13.1.jar" -proc:none -d "/app/client-bytecode/$EXEC_ID" "/app/client-src/$EXEC_ID/$CLASS_NAME.java" 2>"/app/error-log/$EXEC_ID/err.log"
EOF

cat > "/tmp/compiler-rootfs/app/sanity-check.sh" << 'EOF'
#!/bin/sh

sleep 5s
ping -c 1 127.0.0.1 || echo "Ping failed"

JSON='{"SrcCodeB64": "cHVibGljIGNsYXNzIE1haW57CiAgICBwdWJsaWMgc3RhdGljIHZvaWQgbWFpbihTdHJpbmdbXSBhcmdzKXsKICAgICAgICBTeXN0ZW0ub3V0LnByaW50bG4oIkhlbGxvIHByb3h5Iik7CiAgICB9Cn0=","ClassName": "Main","ExecutionId": "123e4567-e89b-12d3-a456-426614174000"}'
LEN=${#JSON}

printf "POST /compile HTTP/1.1\r\nHost: 127.0.0.1\r\nContent-Length: %s\r\nContent-Type: application/json\r\nConnection: close\r\n\r\n%s" "$LEN" "$JSON" | nc 127.0.0.1 5137

EOF


cat > "/tmp/compiler-rootfs/app/proxy.sh" << 'EOF'
#!/bin/sh

while ! nc -z 127.0.0.1 5137; do
  sleep 1s
done

while true; do
  socat VSOCK-LISTEN:5050,fork EXEC:"/bin/sh -c /app/process-input.sh"
done
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
  endpoint=$(echo $payload | jq -r '.endpoint // "health"')
  method=$(echo $payload | jq -r '.method // "GET"')
  content=$(echo $payload | jq -r '.content // "{}"')
  ctype=$(echo $payload | jq -r '.ctype // ""')
  exec_id=$(echo $content | jq -r '.ExecutionId // ""')
  
  #TODO this could use cleaning up however using printf to do a quasi string builder resulted in newlines being interpreted literally and not in compliance with http standard 
  if [ "$content" != "{}" ]; then
    clen=${#content}
    if [ "$ctype" != "" ]; then 
      response=$(printf "%s /%s HTTP/1.1\r\nHost: 127.0.0.1\r\nConnection: close\r\nContent-Type: %s\r\nContent-Length: %s\r\n\r\n%s" "$method" "$endpoint" "$ctype" "$clen" "$content" | nc 127.0.0.1 5137)
    else
      response=$(printf "%s /%s HTTP/1.1\r\nHost: 127.0.0.1\r\nConnection: close\r\nContent-Length: %s\r\n\r\n%s" "$method" "$endpoint" "$clen" "$content" | nc 127.0.0.1 5137)
    fi
  else
    response=$(printf "%s /%s HTTP/1.1\r\nHost: 127.0.0.1\r\nConnection: close\r\n\r\n" "$method" "$endpoint" | nc 127.0.0.1 5137)
  fi
  printf "%s" "$response"
  printf '\004'
done
EOF

chmod +x /tmp/compiler-rootfs/app/sanity-check.sh
chmod +x /tmp/compiler-rootfs/app/proxy.sh
chmod +x /tmp/compiler-rootfs/app/process-input.sh

mount -t proc proc proc/
mount -t sysfs sys sys/
mount -o bind /dev dev/
mount -o bind /dev/pts dev/pts/

chroot /tmp/compiler-rootfs /bin/sh << 'EOF'
apk update
apk add openjdk17-jdk coreutils openrc mdevd curl socat jq netcat-openbsd net-tools

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

INNER_EOF

cat > "/etc/init.d/lo" << 'INNER_EOF'
#!/sbin/openrc-run

depend() {
    need localmount
    before net
    before entrypoint
}

start() {
    ebegin "Setting up loopback interface"
    ip link set lo up
    ip addr add 127.0.0.1/8 dev lo 2>/dev/null || true
    
    sleep 1
    
    ping -c 1 127.0.0.1 >/dev/null 2>&1
    eend $?
}

stop() {
    ebegin "Shutting down loopback interface"
    ip link set lo down
    eend $?
}
INNER_EOF

cat > "/etc/init.d/sanity_check" << 'INNER_EOF'
#!/sbin/openrc-run

description="sanity check"
command="/app/sanity-check.sh"
command_background=false
pidfile="/run/sanity-check.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
    after proxy
}

INNER_EOF

chmod +x /etc/init.d/lo
rc-update add lo boot

chmod +x /etc/init.d/entrypoint
rc-update add entrypoint default

#chmod +x /etc/init.d/sanity_check
#rc-update add sanity_check default

chmod +x /etc/init.d/proxy
rc-update add proxy default
EOF

cd ~/

umount /tmp/compiler-rootfs/dev/pts
umount /tmp/compiler-rootfs/dev
umount /tmp/compiler-rootfs/proc
umount /tmp/compiler-rootfs/sys
umount /tmp/compiler-rootfs

rm -rf /tmp/compiler-rootfs
mv /tmp/compiler-fs.ext4 $STARTING_DIR