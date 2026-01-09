#!/bin/bash
set -e

FIRECRACKER_DIR="/app/firecracker/images"
S3_BUCKET="${S3SETTINGS__BUCKETNAME}"

FILES=(
    "executor-fs.ext4"
    "compiler-fs.ext4"
)

for file in "${FILES[@]}"; do
    local_path="${FIRECRACKER_DIR}/${file}"
    s3_path="s3://${S3_BUCKET}/VmImages/${file}"

    if [ -f "${local_path}" ]; then
        echo "${file} already exists skipping pull"
    else
        echo "Downloading ${s3_path} from S3..."
        aws s3 cp "${s3_path}" "${local_path}"
    fi
done


exec dotnet ExecutorService.dll