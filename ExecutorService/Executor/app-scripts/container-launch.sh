#!/bin/bash

CONTAINER_COUNT=$(yq eval '.COMPILATION_HANDLER.BASE_COUNT' /app/ExecutorConfig.yml)
BASE_PORT=$(yq eval '.COMPILATION_HANDLER.BASE_PORT' /app/ExecutorConfig.yml)
CPUS=$(yq eval '.COMPILATION_HANDLER.COMPILER_CONFIG.CPUS' /app/ExecutorConfig.yml)
MEM=$(yq eval '.COMPILATION_HANDLER.COMPILER_CONFIG.MEM' /app/ExecutorConfig.yml)

for i in $(seq 0 $(($CONTAINER_COUNT-1)));
do
  PORT=$(($BASE_PORT+$i))
#  TODO include auth here?
  curl -X POST http://warden:7139/container -H "Content-Type: application/json" -d "{\"Port\": \"$PORT\", \"Mem\": \"$MEM\", \"Cpus\": \"$CPUS\"}"
done
