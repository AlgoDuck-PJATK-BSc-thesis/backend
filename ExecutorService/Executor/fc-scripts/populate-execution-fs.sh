#!/bin/bash

CLASSNAME="$1"
EXEC_ID="$2"

BYTECODE_DIR="/tmp/$EXEC_ID/bytecode"
ROOTFS_DIR_STAGING="/app/data/$EXEC_ID-rootfs"

if [ ! -d "$BYTECODE_DIR" ] || [ -z "$(ls -A "$BYTECODE_DIR" 2>/dev/null)" ]; then
  curl -X 'DELETE' \
    "http://warden:7139/umount?executionId=$EXEC_ID" \
    -H 'accept: */*' \
    -d ''
  exit 1
fi

mkdir -p "$ROOTFS_DIR_STAGING/sandbox/"
find "$BYTECODE_DIR" -maxdepth 1 -type f -exec mv {} "$ROOTFS_DIR_STAGING/sandbox/" \;

sync  
wait 1

curl -X 'DELETE' \
    "http://warden:7139/umount?executionId=$EXEC_ID" \
    -H 'accept: */*' \
    -d ''