#!/bin/bash

EXEC_ID=$1
MOUNT_DIR="/app/data/$EXEC_ID-rootfs"
BASE_FS_PATH="/app/data/$EXEC_ID-rootfs.ext4"

umount $BASE_FS_PATH