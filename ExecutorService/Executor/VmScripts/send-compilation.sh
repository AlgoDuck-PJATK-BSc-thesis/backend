#!/bin/bash

CODE_JSON=$1
COMPILER_ID=$2
EXEC_ID=$(echo "$CODE_JSON" | jq -r '.content' | jq -r '.ExecutionId')

COMPILER_VSOCK_PATH="/tmp/comp-$COMPILER_ID-firecracker.vsock"
COMPILER_IN_FIFO="/tmp/$EXEC_ID-vsock-pipe"
COMPILER_OUT_FIFO="/tmp/$EXEC_ID-vsock-response-pipe"

mkfifo $COMPILER_IN_FIFO
mkfifo $COMPILER_OUT_FIFO

socat - UNIX-CONNECT:$COMPILER_VSOCK_PATH < $COMPILER_IN_FIFO > $COMPILER_OUT_FIFO &
SOCAT_PID=$!

exec 3>$COMPILER_IN_FIFO
exec 4<$COMPILER_OUT_FIFO

printf 'CONNECT 5050\n' >&3
printf '%s\004' "$CODE_JSON" >&3

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
rm -f $COMPILER_IN_FIFO $COMPILER_OUT_FIFO

pos=$(expr index "$response" '{')
echo "${response:$((pos-1))}" > "/tmp/$EXEC_ID-bytecode.json"

COMP_ERROR=$(jq -r '.ErrorMsg // ""' "/tmp/$EXEC_ID-bytecode.json")

if [[ $COMP_ERROR != "" ]]; then
  echo $COMP_ERROR
  exit 56
fi