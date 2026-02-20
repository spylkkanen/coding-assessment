# Build Agent

You are helping build, run, and verify the OrderTransformer .NET project.

## Build Commands

```bash
# Build the main application
dotnet build src/OrderTransformer

# Build the test project
dotnet test tests/OrderTransformer.Tests --no-run

# Build both projects
dotnet build src/OrderTransformer && dotnet build tests/OrderTransformer.Tests
```

## Run Commands

```bash
# Start infrastructure (Azurite blob storage)
docker compose up -d

# Wait for initialization to complete
docker compose logs azurite-init --follow

# Run the application locally (connects to Azurite on localhost:10000)
dotnet run --project src/OrderTransformer

# Run everything in Docker (including the app)
docker compose --profile app up --build -d

# View app logs in Docker
docker compose --profile app logs order-transformer --follow
```

## Test Commands

```bash
# Run all tests
dotnet test tests/OrderTransformer.Tests

# Run tests with detailed output
dotnet test tests/OrderTransformer.Tests --verbosity normal

# Run only validator tests
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~OrderValidator"

# Run only mapping tests
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~FieldMapping"

# Run only pipeline tests
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~Pipeline"
```

## Verify End-to-End

```bash
# 1. Start Azurite with seed data
docker compose up -d

# 2. Run the app (will process the seed XML file)
dotnet run --project src/OrderTransformer

# 3. Check that output was created in blob storage
docker compose exec azurite-init az storage blob list \
  --container-name orders \
  --prefix output/ \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;" \
  --output table

# 4. Download and inspect the JSON output
docker compose exec azurite-init az storage blob download \
  --container-name orders \
  --name "output/order-batch-001.json" \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;" \
  --file /dev/stdout 2>/dev/null
```

## Clean Up

```bash
# Stop all containers
docker compose --profile app down

# Stop and remove volumes
docker compose --profile app down -v

# Clean build artifacts
dotnet clean src/OrderTransformer
dotnet clean tests/OrderTransformer.Tests
```

## Common Build Issues

- **Package restore fails**: Run `dotnet restore src/OrderTransformer` explicitly
- **Test discovery fails**: Ensure `using Xunit;` is present in test files
- **Azurite connection refused**: Ensure `docker compose up -d` has completed and Azurite is healthy
- **Port conflict on 10000**: Stop other Azurite instances or change port in docker-compose.yml and appsettings.json
