#!/bin/bash

STARTING_DIR=$(pwd)
cd /tmp

rm -rf /tmp/rootfs-alp
rm -rf alpine-rootfs.ext4

#note that seek 192 didn't work so I feel that 256 may be optimal

# TODO skip creating journal and maybe do that cool no root disk space trick
dd if=/dev/zero of=alpine-rootfs.ext4 bs=1M count=0 seek=256
mkfs.ext4 alpine-rootfs.ext4

mkdir -p /tmp/rootfs-alp

mount alpine-rootfs.ext4 /tmp/rootfs-alp

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

chroot /tmp/rootfs-alp /bin/sh << 'EOF'
apk update
apk add openjdk17-jre-headless coreutils openrc mdevd

echo 'ttyS0 root:root 660' > /etc/mdevd.conf

cat > "/etc/init.d/executor" << 'INNER_EOF'
#!/sbin/openrc-run
description="java executor script"
command="/sandbox/run.sh"
command_background=false
pidfile="/run/executor.pid"
start_stop_daemon_args="--make-pidfile"

depend(){
    need localmount
    need mdevd
}

INNER_EOF

chmod +x /etc/init.d/executor
rc-update add executor default
EOF

echo "" >  /tmp/rootfs-alp/etc/resolv.conf

cat > /tmp/rootfs-alp/etc/inittab << EOF
# /etc/inittab

::sysinit:/sbin/openrc --quiet sysinit
::sysinit:/sbin/openrc --quiet boot
::wait:/sbin/openrc --quiet default

# Set up a couple of getty's
tty1::respawn:/sbin/getty 38400 tty1
tty2::respawn:/sbin/getty 38400 tty2
tty3::respawn:/sbin/getty 38400 tty3
tty4::respawn:/sbin/getty 38400 tty4
tty5::respawn:/sbin/getty 38400 tty5
tty6::respawn:/sbin/getty 38400 tty6

# Put a getty on the serial port
#ttyS0::respawn:/sbin/getty -L 115200 ttyS0 vt100

# Stuff to do for the 3-finger salute
::ctrlaltdel:/sbin/reboot

# Stuff to do before rebooting
::shutdown:/sbin/openrc --quiet shutdown

EOF

cd ~ || cd / || exit 1 

umount /tmp/rootfs-alp/dev/pts
umount /tmp/rootfs-alp/dev
umount /tmp/rootfs-alp/proc
umount /tmp/rootfs-alp/sys
umount /tmp/rootfs-alp

rm -rf /tmp/rootfs-alp
mv /tmp/alpine-rootfs.ext4 $STARTING_DIR
