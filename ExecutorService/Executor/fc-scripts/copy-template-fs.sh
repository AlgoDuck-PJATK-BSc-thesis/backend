#!/bin/bash

CLASSNAME="$1"
EXEC_ID="$2"
SIGNING_KEY="$3"

ROOTFS_DIR_STAGING="/app/data/$EXEC_ID-rootfs"
ROOTFS_STAGING="/app/data/$EXEC_ID-rootfs.ext4"

if ! curl -X 'POST' \
  "http://warden:7139/execution-fs?executionId=$EXEC_ID" \
  -H 'accept: */*' \
  -d '' \
  --fail; then
  exit 1
fi

cat > "$ROOTFS_DIR_STAGING/sandbox/run.sh" << EOF
#!/bin/sh
cd /sandbox

time java -cp ".:gson-2.13.1.jar" $CLASSNAME > /dev/ttyS0 2>&1
sync

echo "ctr-${SIGNING_KEY}-pof" > /dev/ttyS0 2>&1

echo 1 > /proc/sys/kernel/sysrq
echo c > /proc/sysrq-trigger

EOF

chmod a-w "$ROOTFS_DIR_STAGING/sandbox/run.sh"
chmod a+x "$ROOTFS_DIR_STAGING/sandbox/run.sh"