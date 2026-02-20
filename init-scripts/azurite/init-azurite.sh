#!/bin/bash
set -e

AZURITE_HOST="${AZURITE_HOST:-localhost}"
AZURITE_BLOB_PORT="${AZURITE_BLOB_PORT:-10000}"

CONNECTION_STRING="DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://${AZURITE_HOST}:${AZURITE_BLOB_PORT}/devstoreaccount1;"

echo "Waiting for Azurite to be ready..."
for i in $(seq 1 30); do
    if az storage container list --connection-string "$CONNECTION_STRING" > /dev/null 2>&1; then
        echo "Azurite is ready"
        break
    fi
    echo "Attempt $i/30 - waiting..."
    sleep 2
done

echo "Creating 'orders' container..."
az storage container create \
    --name orders \
    --connection-string "$CONNECTION_STRING" \
    --output none 2>/dev/null || true

echo "Uploading seed data..."
az storage blob upload \
    --container-name orders \
    --name "input/order-batch-001.xml" \
    --file /seed-data/order-batch-001.xml \
    --connection-string "$CONNECTION_STRING" \
    --overwrite \
    --output none

echo "Verifying upload..."
az storage blob list \
    --container-name orders \
    --connection-string "$CONNECTION_STRING" \
    --output table

echo "Azurite initialization complete!"
