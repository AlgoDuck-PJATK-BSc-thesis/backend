#!/bin/bash

for i in {1..100};do
curl -X 'POST' \
  'http://localhost:1337/api/execute/dry' \
  -H 'accept: */*' \
  -H 'Content-Type: application/json' \
  -d '{
  "codeB64": "cHVibGljIGNsYXNzIE1haW4gewogICAgcHVibGljIHN0YXRpYyB2b2lkIG1haW4oU3RyaW5nW10gYXJncyl7CiAgICAgICAgU3lzdGVtLm91dC5wcmludGxuKCJIZWxsbyBobW0iKTsKICAgIH0KfQ==",
  "lang": "java"
}' &

done
