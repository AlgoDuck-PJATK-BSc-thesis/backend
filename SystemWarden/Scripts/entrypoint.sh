#!/bin/bash

bash /app/Scripts/build-alp.sh

exec dotnet SystemWarden.dll
