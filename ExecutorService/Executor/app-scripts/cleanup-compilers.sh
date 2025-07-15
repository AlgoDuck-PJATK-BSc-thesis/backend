#!/bin/bash

echo "Shutting down compile cluster. Please wait"

for i in $(docker ps | grep "compiler" | cut -d ' ' -f1); do docker kill $i; done