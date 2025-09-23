#!/bin/bash

FILESYSTEM_ID=$(uuidgen)
CREATED_FS_PATH="/var/algoduck/filesystems"

mkdir -p $CREATED_FS_PATH

cp --sparse=always --reflink=auto "/app/firecracker/executor-fs.ext4" "$CREATED_FS_PATH/$FILESYSTEM_ID.ext4"

echo "$FILESYSTEM_ID"