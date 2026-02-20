# Debug Agent

You are helping diagnose and fix issues in the OrderTransformer .NET project.

## Common Issues and Solutions

### Build Errors
```bash
dotnet build src/OrderTransformer 2>&1
```
- **Missing namespace**: Ensure `using OrderTransformer.Models;` and `using OrderTransformer.Services;` are present
- **Record with-expression errors**: Records use `init` setters. Use `record with { Property = newValue }` syntax
- **Nullable reference warnings**: Use null-coalescing (`??`) or null-conditional (`?.`) operators

### Test Failures
```bash
dotnet test tests/OrderTransformer.Tests --verbosity detailed 2>&1
```
- **Test discovery issues**: Ensure test class is `public` and methods have `[Fact]` or `[Theory]` attributes
- **Assertion failures**: Check expected vs actual values in the test output
- **Missing test data**: Verify XML test files are copied to output (`CopyToOutputDirectory` in .csproj)

### Runtime / Blob Storage Issues
```bash
# Check if Azurite is running
docker compose ps

# View Azurite logs
docker compose logs azurite

# Check init script completed
docker compose logs azurite-init

# List blobs in container
docker compose exec azurite-init az storage blob list \
  --container-name orders \
  --connection-string "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://localhost:10000/devstoreaccount1;" \
  --output table
```

### XML Parsing Issues
- **Namespace mismatch**: XML uses namespace `http://example.com/schemas/order/v1`. The parser defines `XNamespace Ns` for element access
- **Missing elements**: Check if the XML structure matches the expected schema (`init-scripts/azurite/seed-data/order-batch-001.xml`)
- **Encoding**: XML files must be UTF-8 encoded

### Pipeline Issues
- Check `TransformationPipeline.cs` for the processing flow
- Validation errors should NOT stop the pipeline - they are collected and included in output
- Field mapping happens AFTER validation
- Output JSON goes to `output/` prefix, processed XML moves to `processed/` prefix

## Project Structure Quick Reference
```
src/OrderTransformer/
  Program.cs                          # Entry point, DI setup
  Worker/BlobPollingWorker.cs         # Polls blob storage
  Worker/TransformationPipeline.cs    # Orchestrates processing
  Services/OrderValidatorService.cs   # STUB - candidate implements
  Services/FieldMappingService.cs     # STUB - candidate implements
  Models/OrderModels.cs               # Domain models
  Models/ValidationError.cs           # Validation error record
```
