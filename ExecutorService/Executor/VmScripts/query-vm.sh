#!/bin/bash
set -euo pipefail

VM_ID=$1
QUERY=$2
TIMEOUT=${3:-15}  

VM_VSOCK_PATH="/var/algoduck/vsocks/$VM_ID.vsock"
QUERY_ID=$(uuidgen)
VM_IN_FIFO="/tmp/$QUERY_ID-in-pipe"
VM_OUT_FIFO="/tmp/$QUERY_ID-out-pipe"
OUTPUT_FILE="/tmp/$VM_ID-out.json"

cleanup() {
    if [[ -n "${SOCAT_PID:-}" ]]; then
        kill "$SOCAT_PID" 2>/dev/null || true
        wait "$SOCAT_PID" 2>/dev/null || true
    fi
    
    exec 3>&- 2>/dev/null || true
    exec 4<&- 2>/dev/null || true
    
    rm -f "$VM_IN_FIFO" "$VM_OUT_FIFO" 2>/dev/null || true
}
trap cleanup EXIT

if [[ ! -S "$VM_VSOCK_PATH" ]]; then
    echo "ERROR: Vsock not found at $VM_VSOCK_PATH" >&2
    exit 1
fi

mkfifo "$VM_IN_FIFO"
mkfifo "$VM_OUT_FIFO"

timeout "$TIMEOUT" socat - "UNIX-CONNECT:$VM_VSOCK_PATH" < "$VM_IN_FIFO" > "$VM_OUT_FIFO" &
SOCAT_PID=$!

exec 3>"$VM_IN_FIFO"
exec 4<"$VM_OUT_FIFO"

printf 'CONNECT 5050\n' >&3
printf '%s\004' "$QUERY" >&3

response=""
read_complete=false

while IFS= read -r -n1 -t "$TIMEOUT" char <&4; do
    if [[ -z "$char" ]]; then
        continue
    fi
    
    char_code=$(printf '%d' "'$char" 2>/dev/null || echo "0")
    if [[ "$char_code" == "4" ]]; then
        read_complete=true
        break
    fi
    
    response="$response$char"
done

if [[ "$read_complete" != "true" ]]; then
    echo "ERROR: Incomplete response from VM (timeout or connection closed)" >&2
    exit 1
fi

if [[ "$response" =~ \{.*\} ]]; then
    json_start="${response#*\{}"
    json_content="{$json_start"
    
    if echo "$json_content" | jq . > /dev/null 2>&1; then
        echo "$json_content" > "$OUTPUT_FILE"
    else
        echo "ERROR: Invalid JSON in response" >&2
        echo "Response was: $response" >&2
        exit 1
    fi
else
    echo "ERROR: No JSON found in response" >&2
    echo "Response was: $response" >&2
    exit 1
fi