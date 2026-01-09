#!/bin/bash

STARTING_DIR=$(pwd)
cd /tmp

rm -rf /tmp/rootfs-alp
rm -rf executor-fs.ext4

dd if=/dev/zero of=executor-fs.ext4 bs=1M count=0 seek=256
mkfs.ext4 executor-fs.ext4

mkdir -p /tmp/rootfs-alp

mount executor-fs.ext4 /tmp/rootfs-alp

cd /tmp/rootfs-alp || { umount /tmp/rootfs-alp && exit 1; }

curl -O https://dl-cdn.alpinelinux.org/alpine/v3.21/releases/x86_64/alpine-minirootfs-3.21.3-x86_64.tar.gz
tar -xpf alpine-minirootfs-3.21.3-x86_64.tar.gz
rm -rf alpine-minirootfs-3.21.3-x86_64.tar.gz

mkdir -p sandbox

curl -o "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar" https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar

chmod a-w "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar"
chmod a+r "/tmp/rootfs-alp/sandbox/gson-2.13.1.jar"

cp /etc/resolv.conf /tmp/rootfs-alp/etc/resolv.conf

cat > "/tmp/rootfs-alp/etc/apk/repositories" << EOF
http://dl-cdn.alpinelinux.org/alpine/v3.21/main
http://dl-cdn.alpinelinux.org/alpine/v3.21/community
EOF

mount -t proc proc proc/
mount -t sysfs sys sys/
mount -o bind /dev dev/
mount -o bind /dev/pts dev/pts/

cat > "/tmp/rootfs-alp/sandbox/vsock-handler.sh" << 'EOF'
#!/bin/sh

CGROUP_PATH="/sys/fs/cgroup/sandbox"

content=""
while IFS= read -r -n1 char; do
    if [ "$(printf '%d' "'$char")" = "4" ]; then
        break
    fi
    content="$content$char"
done

content_decoded=$(echo "$content" | base64 --decode)
sync

printf "%s" "$content_decoded" > /sandbox/content.json

jq -r '.GeneratedClassFiles | to_entries[] | "\(.key) \(.value)"' /sandbox/content.json |
while read -r file b64; do
    echo "$b64" | base64 --decode > "/sandbox/$file"
done
entrypoint_name=$(jq -r '.Entrypoint' /sandbox/content.json)

start_ns=$(date +%s%N)

echo $$ > "$CGROUP_PATH/cgroup.procs"
/usr/bin/time -v -o /sandbox/time.log java -cp "/sandbox:.:/sandbox/gson-2.13.1.jar" "$entrypoint_name" 1>/sandbox/out.log 2>/sandbox/err.log

exit_code=$?
end_ns=$(date +%s%N)

max_mem_kb=$(grep "Maximum resident set size" /sandbox/time.log | awk '{print $NF}')
max_mem_kb=${max_mem_kb:-0}

out=$(jq -Rs . < /sandbox/out.log)
err=$(jq -Rs . < /sandbox/err.log)

echo "{\"out\":$out, \"err\":$err, \"exitCode\":\"$exit_code\", \"startNs\":\"$start_ns\", \"endNs\":\"$end_ns\", \"maxMemoryKb\":\"$max_mem_kb\"}"

printf '\004'
exit 0
EOF
chmod +x "/tmp/rootfs-alp/sandbox/vsock-handler.sh"

touch "/tmp/rootfs-alp/tmp/exec.log"
cat > "/tmp/rootfs-alp/sandbox/run.sh" << 'EOF'
#!/bin/sh 
socat VSOCK-LISTEN:5050 SYSTEM:"/sandbox/vsock-handler.sh"
EOF

chmod +x "/tmp/rootfs-alp/sandbox/run.sh"

chroot /tmp/rootfs-alp /bin/sh << 'EOF'
apk update
apk add openjdk17-jre-headless coreutils openrc mdevd socat jq

echo 'ttyS0 root:root 660' > /etc/mdevd.conf

cat > "/etc/init.d/cgroup-setup" << 'INNER_EOF'
#!/sbin/openrc-run
description="Setup cgroupv2 for sandbox"

depend() {
    before executor
    need localmount
}

start() {
    if ! mountpoint -q /sys/fs/cgroup; then
        mount -t cgroup2 none /sys/fs/cgroup
    fi
    
    echo "+memory +io +pids" > /sys/fs/cgroup/cgroup.subtree_control 2>/dev/null
    
    mkdir -p /sys/fs/cgroup/sandbox
    
    echo "0" > /sys/fs/cgroup/sandbox/memory.swap.max 2>/dev/null
    
    echo "100" > /sys/fs/cgroup/sandbox/pids.max
    
    ROOT_DEV=$(cat /sys/block/vda/dev 2>/dev/null || echo "254:0")
    echo "$ROOT_DEV wbps=10485760 wiops=50 rbps=20971520 riops=100" > /sys/fs/cgroup/sandbox/io.max 2>/dev/null
    
    eend 0
}
INNER_EOF

chmod +x /etc/init.d/cgroup-setup
rc-update add cgroup-setup boot

cat > "/etc/init.d/executor" << 'INNER_EOF'
#!/sbin/openrc-run
description="java executor script"
command="/sandbox/run.sh"
command_background=true
pidfile="/run/executor.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
    need cgroup-setup
}

start_post(){
    echo "READY" > /dev/ttyS0
    exit 0
}
INNER_EOF

chmod +x /etc/init.d/executor
rc-update add executor default
EOF

echo "" > /tmp/rootfs-alp/etc/resolv.conf

cd ~ || cd / || exit 1 

umount /tmp/rootfs-alp/dev/pts
umount /tmp/rootfs-alp/dev
umount /tmp/rootfs-alp/proc
umount /tmp/rootfs-alp/sys
umount /tmp/rootfs-alp

rm -rf /tmp/rootfs-alp
mv /tmp/executor-fs.ext4 "$STARTING_DIR"