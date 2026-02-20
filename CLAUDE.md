# Order Transformer - Coding Assessment

## Project Overview
This is a .NET 9 Worker Service that monitors Azure Blob Storage (Azurite emulator) for incoming XML order files, transforms them to JSON, and writes output back to blob storage.

## Architecture
- **Pipeline pattern**: `Parse XML → Validate → Map Fields → Transform to JSON → Store`
- **DI-based services**: All services registered via interfaces in `Program.cs`
- **BackgroundService**: `BlobPollingWorker` polls blob storage on a timer

## Key Files
- `src/OrderTransformer/Worker/TransformationPipeline.cs` - Orchestrates the processing pipeline
- `src/OrderTransformer/Worker/BlobPollingWorker.cs` - Polls blob storage for new files
- `src/OrderTransformer/Models/OrderModels.cs` - Domain models (immutable records)
- `src/OrderTransformer/Services/` - All service interfaces and implementations

## Candidate Extension Points (STUBS to implement)
- `Services/OrderValidatorService.cs` - Validate order fields (required fields, format patterns, ranges)
- `Services/FieldMappingService.cs` - Map field values (country codes→names, product codes→categories, status→labels)
- `Tests/Services/OrderValidatorServiceTests.cs` - Unit tests for validation
- `Tests/Services/FieldMappingServiceTests.cs` - Unit tests for mapping

## Commands
```bash
dotnet build src/OrderTransformer                    # Build the app
dotnet test tests/OrderTransformer.Tests             # Run tests
dotnet run --project src/OrderTransformer            # Run locally
docker compose up -d                                 # Start Azurite
docker compose --profile app up --build -d           # Run everything
```

## Conventions
- Use C# records with `init` setters for immutable models
- Follow existing interface + implementation pattern for services
- Use xUnit with `[Fact]` and `[Theory]` for tests
- Use `System.Text.Json` for JSON serialization
- Use `System.Xml.Linq` for XML parsing
- Namespace: `OrderTransformer.Models`, `OrderTransformer.Services`, `OrderTransformer.Worker`
