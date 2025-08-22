#!/bin/bash

EXEC_ID=$1
MOUNT_DIR="/app/data/$EXEC_ID-rootfs"
BASE_FS_PATH="/app/data/$EXEC_ID-rootfs.ext4"

cp --sparse=always --reflink=auto /app/alpine-rootfs.ext4 $BASE_FS_PATH

mkdir -p $MOUNT_DIR 

mkdir -p "$MOUNT_DIR/sandbox"

mount $BASE_FS_PATH $MOUNT_DIR