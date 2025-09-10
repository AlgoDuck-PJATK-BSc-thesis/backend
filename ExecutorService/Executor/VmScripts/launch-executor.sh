#!/bin/bash

EXEC_ID=$1
GUEST_CID=$2

# it is important to note that all executors using the same filesystem is an incredibly temporary solution. One of the highest priority things to do is implementing getting those from a premade, managed pool 
EXEC_ROOTFS="/app/firecracker/executor-fs.ext4"
KERNEL_PATH="/app/firecracker/vmlinux.bin"

CONFIG_FILE="/tmp/vm-config-$EXEC_ID.json"
VSOCK_PATH="/tmp/exec-firecracker-$EXEC_ID.vsock"
PID_PATH="/tmp/$EXEC_ID-pid.pid"

cat > "$CONFIG_FILE" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 init=/sbin/init quiet loglevel=0 selinux=0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd"
  },
  "drives": [
    {
      "drive_id": "rootfs",
      "path_on_host": "$EXEC_ROOTFS",
      "is_root_device": true,
      "is_read_only": false
    }
  ],
  "machine-config": {
    "vcpu_count": 1,
    "mem_size_mib": 256,
    "smt": false
  },
  "vsock": {
    "guest_cid": $GUEST_CID,
    "uds_path": "$VSOCK_PATH"
  }
}
EOF

FC_STDOUT_FILE="/tmp/vm-stdout-$EXEC_ID.log"
touch "$FC_STDOUT_FILE"

timeout 15s firecracker --no-api --config-file "$CONFIG_FILE" > "$FC_STDOUT_FILE" 2>&1 &
FIRECRACKER_PID=$!

echo "$FIRECRACKER_PID" > "$PID_PATH"