#!/bin/bash

VM_ID=$1
GUEST_CID=$2
FILESYSTEM_ID=$3
VCPU_COUNT=$4
MEM=$5
SMT=$6

ALGODUCK="/var/algoduck"
KERNEL_PATH="/app/firecracker/vmlinux.bin"

FILESYSTEMS_PATH="$ALGODUCK/filesystems"
VSOCKS_PATH="$ALGODUCK/vsocks"
CONFIGS_PATH="$ALGODUCK/configs"

mkdir -p $FILESYSTEMS_PATH $VSOCKS_PATH $CONFIGS_PATH

ROOTFS_PATH="$FILESYSTEMS_PATH/$FILESYSTEM_ID.ext4"
VSOCK_PATH="$VSOCKS_PATH/$VM_ID.vsock"
CONFIG_PATH="$CONFIGS_PATH/$VM_ID.json"

cat > "$CONFIG_PATH" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 init=/sbin/init quiet loglevel=0 selinux=0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd"
  },
  "drives": [
    {
      "drive_id": "rootfs",
      "path_on_host": "$ROOTFS_PATH",
      "is_root_device": true,
      "is_read_only": false
    }
  ],
  "machine-config": {
    "vcpu_count": $VCPU_COUNT,
    "mem_size_mib": $MEM,
    "smt": $SMT
  },
  "vsock": {
    "guest_cid": $GUEST_CID,
    "uds_path": "$VSOCK_PATH"
  }
}
EOF

FC_STDOUT_FILE="/tmp/vm-stdout-$VM_ID.log"
touch "$FC_STDOUT_FILE"

firecracker --no-api --config-file "$CONFIG_PATH" > "$FC_STDOUT_FILE" 2>&1 & disown
FIRECRACKER_PID=$!

( tail -f -n +1 "$FC_STDOUT_FILE" & ) | while read -r line; do
    if [[ $line =~ READY ]]; then
        pkill -P $$ tail  
        break
    fi
done

rm -rf $FC_STDOUT_FILE

echo $FIRECRACKER_PID