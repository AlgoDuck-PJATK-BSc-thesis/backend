#!/bin/bash

CLASSNAME="$1"
EXEC_ID="$2"

ROOTFS_DIR="/tmp/$EXEC_ID-rootfs"

if [ ! -f "/tmp/$EXEC_ID.class" ]; then
  echo ".class file not found. Exiting"
  umount "$ROOTFS_DIR"
  exit 1
fi 

cp "/tmp/$EXEC_ID.class" "$ROOTFS_DIR/sandbox/$CLASSNAME.class"

umount "$ROOTFS_DIR"




