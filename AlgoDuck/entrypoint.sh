#!/bin/sh
set -e
mkdir -p /app/keys
chown -R app:app /app/keys
exec gosu app:app dotnet AlgoDuck.dll
