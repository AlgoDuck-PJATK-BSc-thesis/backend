#!/bin/bash

VM_ID=$1
QUERY=$2

QUERY_ID=$(uuidgen)

VM_VSOCK_PATH="/var/algoduck/vsocks/$VM_ID.vsock"

VM_IN_FIFO="/tmp/$QUERY_ID-in-pipe"
VM_OUT_FIFO="/tmp/$QUERY_ID-out-pipe"

mkfifo $VM_IN_FIFO
mkfifo $VM_OUT_FIFO

socat - UNIX-CONNECT:$VM_VSOCK_PATH < $VM_IN_FIFO > $VM_OUT_FIFO &
SOCAT_PID=$!

exec 3>$VM_IN_FIFO
exec 4<$VM_OUT_FIFO

printf 'CONNECT 5050\n' >&3
printf '%s\004' "$QUERY" >&3

response=""
while IFS= read -r -n1 char <&4; do
    if [ "$(printf '%d' "'$char")" = "4" ]; then
        break
    fi
    response="$response$char"
done

exec 3>&-
exec 4<&-
kill $SOCAT_PID 2>/dev/null
wait $SOCAT_PID 2>/dev/null
rm -f $VM_IN_FIFO $VM_OUT_FIFO

pos=$(expr index "$response" '{')
echo "${response:$((pos-1))}" > "/tmp/$VM_ID-out.json"