#!/bin/bash

PORT=$1
MEM=$2
CPUS=$3

CIDFILE_PATH="/tmp/cidfile-$PORT"
rm -f $CIDFILE_PATH
docker run --rm --name "compiler$PORT" -p $PORT:5137 --network backend_app_network --cidfile $CIDFILE_PATH --rm --memory=$MEM --cpus=$CPUS --security-opt=no-new-privileges compiler 1>/dev/null & disown

while [ ! -f $CIDFILE_PATH ]; do
    sleep 0.1
done

CONTAINER_ID=$(cat $CIDFILE_PATH)

CONTINUE_FLAG=0
while [ $CONTINUE_FLAG -eq 0 ]; do
    if docker exec $CONTAINER_ID nc -z localhost 5137 2>/dev/null; then
        CONTINUE_FLAG=1
        break
    fi
    sleep 1
done

echo $CONTAINER_ID