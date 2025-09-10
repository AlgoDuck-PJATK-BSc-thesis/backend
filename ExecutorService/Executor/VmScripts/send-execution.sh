#!/bin/bash

EXEC_ID=$1

VSOCK_PATH="/tmp/exec-firecracker-$EXEC_ID.vsock"
PID_PATH="/tmp/$EXEC_ID-pid.pid"
FC_STDOUT_FILE="/tmp/vm-stdout-$EXEC_ID.log"
BYTECODE_FILE="/tmp/$EXEC_ID-bytecode.json"

touch $FC_STDOUT_FILE

( tail -f -n +1 "$FC_STDOUT_FILE" & ) | while read -r line; do
    if [[ $line =~ READY ]]; then
        pkill -P $$ tail  
        break
    fi
done

FC_IN_FIFO="/tmp/vm-in-pipe-$EXEC_ID"
FC_OUT_FIFO="/tmp/vm-out-pipe-$EXEC_ID"

mkfifo $FC_IN_FIFO
mkfifo $FC_OUT_FIFO

socat - UNIX-CONNECT:$VSOCK_PATH < $FC_IN_FIFO > $FC_OUT_FIFO &

SOCAT_PID=$!

exec 3>$FC_IN_FIFO
exec 4<$FC_OUT_FIFO

printf 'CONNECT 5050\n' >&3;
printf '%s\004' "$(cat $BYTECODE_FILE)" >&3

response=""
while IFS= read -r -n1 char <&4; do
    if [ "$(printf '%d' "'$char")" = "4" ]; then
        break
        kill $(cat $PID_PATH) 2>/dev/null
    fi
    response="$response$char"

done

pos=$(expr index "$response" '{')
echo "${response:$((pos-1))}" > "/tmp/$EXEC_ID-response.json"

exec 3>&-
exec 3>&-
exec 4<&-
kill $SOCAT_PID 2>/dev/null
wait $SOCAT_PID 2>/dev/null

rm -f $FC_IN_FIFO $FC_OUT_FIFO $VSOCK_PATH $FC_STDOUT_FIFO $FC_STDOUT_FILE $PID_PATH $BYTECODE_FILE

wait $FIRECRACKER_PID