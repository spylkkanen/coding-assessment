# Implementation Agent

You are helping implement features in the OrderTransformer .NET project.

## Project Context
This is a data transformation pipeline that processes XML order files from Azure Blob Storage, transforms them to JSON, and writes output back to blob storage.

**Pipeline flow**: `Parse XML → Validate → Map Fields → Transform to JSON → Store`

## Your Task
The candidate needs to implement two stub services:

### 1. OrderValidatorService (`src/OrderTransformer/Services/OrderValidatorService.cs`)
Implement the `Validate(OrderBatch batch)` method with these rules:
- **Required fields**: orderId, orderDate, customerId, name, email must be non-empty
- **OrderId format**: Must match pattern `ORD-YYYY-NNNNNN` (e.g., `ORD-2024-001234`)
- **Email format**: Must contain `@` with text before and after (pattern: `[^@]+@[^@]+\.[^@]+`)
- **Country codes**: Exactly 2 uppercase letters (ISO 3166-1 alpha-2)
- **Currency codes**: Exactly 3 uppercase letters (ISO 4217)
- **Quantities**: Must be positive integers (> 0)
- **Prices/totals**: Must be non-negative (>= 0)

Return a `List<ValidationError>` with OrderId, Field name, Message, and ErrorCode.

### 2. FieldMappingService (`src/OrderTransformer/Services/FieldMappingService.cs`)
Implement the `MapFields(OrderBatch batch)` method with these mappings:
- **Country codes → names**: FI=Finland, SE=Sweden, NO=Norway, DK=Denmark, US=United States, GB=United Kingdom, DE=Germany
- **Product codes → categories**: PROD-001=Widgets, PROD-002=Gadgets, PROD-003=Premium Widgets
- **Status codes → labels**: draft=Draft, confirmed=Order Confirmed, processing=In Processing, shipped=Shipped, delivered=Delivered, cancelled=Cancelled

## Important Patterns
- Use C# records with `with` expressions for immutable updates
- Follow the existing interface pattern in the Services/ folder
- Use `System.Text.RegularExpressions.Regex` for pattern matching
- Reference `src/OrderTransformer/Models/OrderModels.cs` for the domain model structure
- Reference `src/OrderTransformer/Models/ValidationError.cs` for the error record

## Commands
```bash
dotnet build src/OrderTransformer          # Verify it compiles
dotnet test tests/OrderTransformer.Tests   # Run tests
```
