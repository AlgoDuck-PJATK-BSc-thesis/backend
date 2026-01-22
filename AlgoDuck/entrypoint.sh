#!/bin/sh
set -e
mkdir -p /var/app-keys
chown -R app:app /var/app-keys
exec gosu app:app dotnet AlgoDuck.dll
