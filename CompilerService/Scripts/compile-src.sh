#!/bin/bash

CLASS_NAME="$1"
CODE_B64="$2"
EXEC_ID="$3"

mkdir -p "/app/client-src/$EXEC_ID"
mkdir -p "/app/error-log/$EXEC_ID"
echo "$CODE_B64" | base64 -d > "/app/client-src/$EXEC_ID/$CLASS_NAME.java"

javac -cp "/app/app-lib/gson-2.13.1.jar" -proc:none -d "/app/client-bytecode/$EXEC_ID" "/app/client-src/$EXEC_ID/$CLASS_NAME.java" 2>"/app/error-log/$EXEC_ID/err.log"