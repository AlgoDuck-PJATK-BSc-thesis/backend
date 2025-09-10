#!/bin/bash

COMPILER_ID=$1
COMPILER_CID=$2

KERNEL_PATH="/app/firecracker/vmlinux.bin"
CONFIG_FILE="/tmp/compiler-vm-config.json"
# it is important to note that all compilers using the same filesystem is an incredibly temporary solution. One of the highest priority things to do is implementing getting those from a premade, managed pool 
ROOTFS="/app/firecracker/compiler-fs.ext4"
VSOCK_PATH="/tmp/comp-$COMPILER_ID-firecracker.vsock"

cat > "$CONFIG_FILE" << EOF
{
  "boot-source": {
    "kernel_image_path": "$KERNEL_PATH",
    "boot_args": "console=ttyS0 init=/sbin/init quiet loglevel=0 selinux=0 reboot=k panic=-1 pci=off nomodules i8042.noaux i8042.nomux i8042.nopnp i8042.nokbd random.trust_cpu=on"
  },
  "drives": [
    {
      "drive_id": "rootfs",
      "path_on_host": "$ROOTFS",
      "is_root_device": true,
      "is_read_only": false
    }
  ],
  "machine-config": {
    "vcpu_count": 1,
    "mem_size_mib": 2048,
    "smt": false
  },
  "vsock": {
    "guest_cid": $COMPILER_CID,
    "uds_path": "$VSOCK_PATH"
  }
}
EOF

firecracker --no-api --config-file "$CONFIG_FILE" > /dev/null