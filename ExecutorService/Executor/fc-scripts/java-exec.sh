#!/bin/bash

EXEC_ID=$1
SIGNING_KEY=$2

ROOTFS_STAGING="/app/data/$EXEC_ID-rootfs.ext4"
ROOTFS_STAGING_DIR="/app/data/$EXEC_ID-rootfs"

KERNEL_PATH="/app/fc-scripts/vmlinux.bin"
CONFIG_FILE="/tmp/vm_config-$EXEC_ID.json"
SOCK_PATH="/tmp/firecracker-${EXEC_ID}.socket"

STDOUT_PATH="/tmp/$EXEC_ID-OUT-LOG.log"
ANSW_PATH="/tmp/$EXEC_ID-ANSW-LOG.log"
TIME_PATH="/tmp/$EXEC_ID-TIME-LOG.log"
TMP_PATH="/tmp/$EXEC_ID-tmp.txt"
PID_PATH="/tmp/$EXEC_ID-pid"
FIFO_PATH="/tmp$EXEC_ID-fifo.fifo"

touch "$ANSW_PATH" "$STDOUT_PATH" "$TIME_PATH"

mkfifo "$FIFO_PATH"

cat > "$CONFIG_FILE" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 init=/sbin/init quiet loglevel=0 selinux=0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd"
  },
  "drives": [
    {
      "drive_id": "rootfs",
      "path_on_host": "$ROOTFS_STAGING",
      "is_root_device": true,
      "is_read_only": false
    }
  ],
  "machine-config": {
    "vcpu_count": 1,
    "mem_size_mib": 256,
    "smt": false
  }
}
EOF

process_control_output(){
  while read -r line; do
    echo "$line" > /dev/ttyS0 2>&1
    if [[ "$line" =~ ctr-"$SIGNING_KEY"- ]]; then
      if [[ "$line" =~ pof ]]; then
        FIRECRACKER_PID=$(cat "$PID_PATH")
        kill -s SIGKILL $FIRECRACKER_PID
      elif [[ "$line" =~ ans ]]; then
        echo "$line" >> "$ANSW_PATH"
      elif [[ "$line" =~ time ]]; then
        echo "$line" >> "$TIME_PATH"
      fi
    else
        echo "$line" >> "$STDOUT_PATH"
    fi
  done
}

process_control_output < "$FIFO_PATH" &
PROCESSOR_PID=$!

firecracker-v1.2.0-x86_64 --api-sock "$SOCK_PATH" --config-file "$CONFIG_FILE" 2>&1 > "$FIFO_PATH" &
FIRECRACKER_PID=$!
echo $FIRECRACKER_PID > $PID_PATH

wait $FIRECRACKER_PID

if [ $? -eq 137 ]; then
    echo "timed out" >> "$STDOUT_PATH"
fi

tail -n +4 "$STDOUT_PATH" | head -n -1 > "$TMP_PATH" && mv "$TMP_PATH" "$STDOUT_PATH"
rm -rf "$SOCK_PATH" "$CONFIG_FILE" "$ROOTFS" "$CLASS_PATH" "$ROOTFS_DIR" "$ROOTFS_STAGING" "$ROOTFS_STAGING_DIR" "$FIFO_PATH" "$PID_PATH" & disown