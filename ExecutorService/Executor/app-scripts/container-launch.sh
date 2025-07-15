#!/bin/bash

# TODO read these from .env
CONTAINER_COUNT=5
BASE_PORT=5137

for i in $(seq 0 $(($CONTAINER_COUNT-1)));
do
  PORT=$(($BASE_PORT+$i))
  if [ $( docker ps | grep -c "$PORT->$BASE_PORT/tcp" ) -gt 0 ]; then
    echo "container found on port: $PORT. shutting down"
    docker kill $(docker ps | grep "$PORT->$BASE_PORT/tcp" | cut -d ' ' -f1)
  fi
  docker run -p $PORT:$BASE_PORT compiler 1>/dev/null & disown 
done

CONTINUE_FLAG=0
while [ $CONTINUE_FLAG -eq 0 ]; 
do
  CONTINUE_FLAG=1
  for i in $(seq 0 $(($CONTAINER_COUNT-1)));
  do
    PORT=$(($BASE_PORT+$i))
    if ! nc -z $HOST_NAME $PORT; then
      CONTINUE_FLAG=0
      break
    fi  
  done
  echo "waiting..."
  sleep 1
done

echo "services ready"