#!/bin/bash

#directory changing necessary for now as build-alp.sh uses relative paths
#cd fc-scripts
#bash build-alp.sh
#cd ..

bash /app/app-scripts/container-launch.sh

exec dotnet ExecutorService.dll