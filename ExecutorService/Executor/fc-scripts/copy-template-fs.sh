#!/bin/bash

CLASSNAME="$1"
EXEC_ID="$2"
SIGNING_KEY="$3"

ROOTFS_DIR="/tmp/$EXEC_ID-rootfs"
ROOTFS="/tmp/$EXEC_ID-rootfs.ext4"

mkdir -p "$ROOTFS_DIR"

cp --reflink=auto --sparse=always "/app/fc-scripts/alpine-rootfs.ext4" "$ROOTFS"

mount "$ROOTFS" "$ROOTFS_DIR"

cat > "$ROOTFS_DIR/sandbox/run.sh" << EOF
#!/bin/sh
cd /sandbox

time java -cp ".:gson-2.13.1.jar" $CLASSNAME > /dev/ttyS0 2>&1
sync

echo "ctr-${SIGNING_KEY}-pof" > /dev/ttyS0 2>&1

echo 1 > /proc/sys/kernel/sysrq
echo c > /proc/sysrq-trigger

EOF

chmod a-w "$ROOTFS_DIR/sandbox/run.sh"
chmod a+x "$ROOTFS_DIR/sandbox/run.sh"
