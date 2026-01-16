#!/bin/bash

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

cp /etc/resolv.conf /tmp/compiler-rootfs/etc/resolv.conf
cat > "/tmp/compiler-rootfs/etc/apk/repositories" << EOF
http://dl-cdn.alpinelinux.org/alpine/v3.21/main
http://dl-cdn.alpinelinux.org/alpine/v3.21/community
EOF

mkdir -p /tmp/compiler-rootfs/app/lib

cp "$STARTING_DIR/CompilationHandler.py" "/tmp/compiler-rootfs/app/main.py"

curl -o "/tmp/compiler-rootfs/app/lib/gson-2.13.1.jar" \
    https://repo1.maven.org/maven2/com/google/code/gson/gson/2.13.1/gson-2.13.1.jar
chmod 444 "/tmp/compiler-rootfs/app/lib/gson-2.13.1.jar"

mount -t proc proc proc/
mount -t sysfs sys sys/
mount -o bind /dev dev/
mount -o bind /dev/pts dev/pts/

chroot /tmp/compiler-rootfs /bin/sh << 'EOF'
apk update
apk add openjdk17-jdk coreutils openrc mdevd python3

echo 'ttyS0 root:root 660' > /etc/mdevd.conf

cat > "/etc/init.d/entrypoint" << 'INNER_EOF'
#!/sbin/openrc-run
description="compilation handler"
command="/usr/bin/python3"
command_args="/app/main.py"
command_background=true
pidfile="/run/entrypoint.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
}

INNER_EOF

chmod +x /etc/init.d/entrypoint
rc-update add entrypoint default
EOF

cd ~/

umount /tmp/compiler-rootfs/dev/pts
umount /tmp/compiler-rootfs/dev
umount /tmp/compiler-rootfs/proc
umount /tmp/compiler-rootfs/sys
umount /tmp/compiler-rootfs

rm -rf /tmp/compiler-rootfs
mv /tmp/compiler-fs.ext4 "$STARTING_DIR"