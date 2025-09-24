#!/bin/bash

BASE_IMAGE_TYPE_NAME=$1

FILESYSTEM_ID=$(uuidgen)
CREATED_FS_PATH="/var/algoduck/filesystems"

mkdir -p $CREATED_FS_PATH

cp --sparse=always --reflink=auto "/app/firecracker/$BASE_IMAGE_TYPE_NAME-fs.ext4" "$CREATED_FS_PATH/$FILESYSTEM_ID.ext4"

echo "$FILESYSTEM_ID"