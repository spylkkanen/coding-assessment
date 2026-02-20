# Order Transformer

A .NET 9 Worker Service that monitors Azure Blob Storage for incoming XML order files, transforms them to JSON, and stores the output back to blob storage.

## Architecture

```
Azurite Blob Storage
  input/  ──────────────────────────┐
                                    │
                         BlobPollingWorker
                         (polls every 5s)
                                    │
                        TransformationPipeline
                                    │
                    ┌───────────────┼───────────────┐
                    │               │               │
              XmlParserService  OrderValidator  FieldMapping
              (XML → Model)    Service          Service
                    │          (validate)       (map values)
                    │               │               │
                    └───────────────┼───────────────┘
                                    │
                        JsonTransformerService
                        (Model → JSON)
                                    │
  output/ ◄─────────────────────────┘
  processed/ ◄── (input file moved here after success)
  failed/    ◄── (input file moved here on error)
```

## Data Flow

1. XML order files are placed in the `input/` folder of the blob storage container
2. `BlobPollingWorker` detects new files every 5 seconds
3. `XmlParserService` parses the XML into `OrderBatch` domain model
4. `OrderValidatorService` validates all fields and collects errors
5. `FieldMappingService` maps field values (codes → human-readable names)
6. `JsonTransformerService` serializes the model to JSON (including any validation errors)
7. JSON output is written to the `output/` folder
8. Processed XML files are moved to `processed/`

## Domain Model

```
OrderBatch
├── TenantId
└── Orders[]
    ├── Header (OrderId, OrderDate, Status)
    ├── Customer (CustomerId, Name, Email, Address)
    ├── Items[] (LineNumber, ProductCode, Description, Quantity, UnitPrice, Currency)
    └── Totals (Subtotal, TaxRate, TaxAmount, Total, Currency)
```

## Project Structure

```
src/OrderTransformer/
├── Program.cs                          # Entry point, DI registration
├── Models/
│   ├── OrderModels.cs                  # Domain model records
│   ├── ValidationError.cs             # Validation error record
│   └── TransformationResult.cs        # Pipeline result record
├── Services/
│   ├── IBlobStorageService.cs         # Blob read/write/move interface
│   ├── BlobStorageService.cs          # Azure.Storage.Blobs implementation
│   ├── IXmlParserService.cs           # XML parsing interface
│   ├── XmlParserService.cs            # System.Xml.Linq implementation
│   ├── IJsonTransformerService.cs     # JSON output interface
│   ├── JsonTransformerService.cs      # System.Text.Json implementation
│   ├── IOrderValidatorService.cs      # Validation interface
│   ├── OrderValidatorService.cs       # Validation implementation
│   ├── IFieldMappingService.cs        # Field mapping interface
│   └── FieldMappingService.cs         # Mapping implementation
└── Worker/
    ├── BlobPollingWorker.cs           # BackgroundService polling blob storage
    └── TransformationPipeline.cs      # Orchestrates the processing steps

tests/OrderTransformer.Tests/
├── Services/
│   ├── XmlParserServiceTests.cs       # XML parsing tests
│   ├── JsonTransformerServiceTests.cs # JSON output tests
│   ├── OrderValidatorServiceTests.cs  # Validation tests
│   └── FieldMappingServiceTests.cs    # Mapping tests
├── Worker/
│   └── TransformationPipelineTests.cs # Pipeline orchestration tests
└── TestData/
    ├── valid-order.xml                # Valid test data
    └── invalid-order.xml              # Invalid test data
```

## Quick Start

### Prerequisites
- .NET 9 SDK
- Docker and Docker Compose

### Start Infrastructure

```bash
# Start Azurite blob storage with seed data
docker compose up -d

# Verify initialization completed
docker compose logs azurite-init
```

### Run Locally

```bash
# Build
dotnet build src/OrderTransformer

# Run (connects to Azurite on localhost:10000)
dotnet run --project src/OrderTransformer
```

### Run Tests

```bash
dotnet test tests/OrderTransformer.Tests
```

### Run in Docker

```bash
docker compose --profile app up --build -d

# View logs
docker compose --profile app logs order-transformer --follow
```

### Verify Output

```bash
# List output blobs
docker compose exec azurite-init az storage blob list \
  --container-name orders \
  --prefix output/ \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;" \
  --output table
```

### Clean Up

```bash
docker compose --profile app down -v
```

## Configuration

Configuration is in `src/OrderTransformer/appsettings.json`:

| Setting | Default | Description |
|---|---|---|
| `BlobStorage:ConnectionString` | Azurite default | Azure Blob Storage connection string |
| `BlobStorage:ContainerName` | `orders` | Blob container name |
| `BlobStorage:InputPrefix` | `input/` | Folder to monitor for new files |
| `BlobStorage:OutputPrefix` | `output/` | Folder to write JSON output |
| `BlobStorage:ProcessedPrefix` | `processed/` | Folder for successfully processed files |
| `BlobStorage:FailedPrefix` | `failed/` | Folder for failed files |
| `BlobStorage:PollingIntervalSeconds` | `5` | How often to check for new files |

## Technology Stack

- **.NET 9** Worker Service
- **Azure.Storage.Blobs** SDK for blob operations
- **System.Xml.Linq** for XML parsing
- **System.Text.Json** for JSON serialization
- **xUnit** + **Moq** for testing
- **Docker Compose** + **Azurite** for local development
