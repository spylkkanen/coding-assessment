# Testing Agent

You are helping write unit tests for the OrderTransformer .NET project.

## Testing Framework
- **xUnit** with `[Fact]` for single cases and `[Theory]` with `[InlineData]` for parameterized tests
- **Arrange-Act-Assert** pattern
- Tests are in `tests/OrderTransformer.Tests/`

## What Needs Testing

### 1. OrderValidatorServiceTests (`tests/OrderTransformer.Tests/Services/OrderValidatorServiceTests.cs`)
Write tests for each validation rule:
- Valid order batch returns no errors
- Missing required fields (orderId, customerId, name, email) each produce an error
- Invalid OrderId format (e.g., "INVALID-ID", "ORD-24-001234") produces FORMAT error
- Invalid email (e.g., "not-an-email", "@missing.com") produces FORMAT error
- Invalid country code (e.g., "finland", "f", "FIN") produces FORMAT error
- Invalid currency code (e.g., "euro", "EU", "euros") produces FORMAT error
- Zero or negative quantity produces RANGE error
- Negative price produces RANGE error

### 2. FieldMappingServiceTests (`tests/OrderTransformer.Tests/Services/FieldMappingServiceTests.cs`)
Write tests for each mapping:
- Country code "FI" maps to "Finland"
- Country code "SE" maps to "Sweden"
- Unknown country code remains unchanged
- Product code "PROD-001" maps to "Widgets"
- Status "confirmed" maps to "Order Confirmed"
- Unknown status remains unchanged
- Multiple orders in a batch are all mapped

## Test Data Helper
Create test OrderBatch objects using C# record constructors. Example pattern from existing tests:

```csharp
private static OrderBatch CreateValidBatch() => new()
{
    TenantId = "test-tenant",
    Orders = new List<Order>
    {
        new()
        {
            Header = new OrderHeader { OrderId = "ORD-2024-001234", OrderDate = "2024-01-15T10:30:00Z", Status = "confirmed" },
            Customer = new Customer { CustomerId = "CUST-001", Name = "Test Corp", Email = "test@example.com",
                Address = new Address { Street = "123 Test St", City = "Helsinki", PostalCode = "00100", Country = "FI" } },
            Items = new List<OrderItem> { new() { LineNumber = 1, ProductCode = "PROD-001", Description = "Widget", Quantity = 10, UnitPrice = 29.99m, Currency = "EUR" } },
            Totals = new OrderTotals { Subtotal = 299.90m, TaxRate = 24m, TaxAmount = 71.98m, Total = 371.88m, Currency = "EUR" }
        }
    }
};
```

## Reference
Look at existing tests for patterns:
- `tests/OrderTransformer.Tests/Services/XmlParserServiceTests.cs`
- `tests/OrderTransformer.Tests/Services/JsonTransformerServiceTests.cs`

## Commands
```bash
dotnet test tests/OrderTransformer.Tests --verbosity normal
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~OrderValidator"
dotnet test tests/OrderTransformer.Tests --filter "FullyQualifiedName~FieldMapping"
```
