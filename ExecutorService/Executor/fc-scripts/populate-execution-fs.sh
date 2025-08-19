#!/bin/bash

CLASSNAME="$1"
EXEC_ID="$2"

ROOTFS_DIR="/tmp/$EXEC_ID-rootfs"
BYTECODE_DIR="/tmp/$EXEC_ID/bytecode"

if [ ! -d "$BYTECODE_DIR" ] || [ -z "$(ls -A "$BYTECODE_DIR" 2>/dev/null)" ]; then
  echo "Bytecode directory not found or empty. Exiting"
  umount "$ROOTFS_DIR" 2>/dev/null || true
  exit 1
fi

mkdir -p "$ROOTFS_DIR/sandbox"


find "$BYTECODE_DIR" -name "*.class" -type f -exec mv {} "$ROOTFS_DIR/sandbox/" \;

if [ -z "$(ls -A "$ROOTFS_DIR/sandbox" 2>/dev/null)" ]; then
  echo "No .class files found to move. Exiting"
  umount "$ROOTFS_DIR" 2>/dev/null || true
  exit 1
fi

echo "Successfully moved $(ls -1 "$ROOTFS_DIR/sandbox"/*.class 2>/dev/null | wc -l) .class files to sandbox"

umount "$ROOTFS_DIR"