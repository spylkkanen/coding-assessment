using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using OrderTransformer.Models;
using OrderTransformer.Services;
using Xunit;

namespace OrderTransformer.Tests.Services;

public class JsonTransformerServiceTests
{
    private readonly JsonTransformerService _sut = new(NullLogger<JsonTransformerService>.Instance);

    private static OrderBatch CreateTestBatch() => new()
    {
        TenantId = "test-tenant",
        Orders = new List<Order>
        {
            new()
            {
                Header = new OrderHeader
                {
                    OrderId = "ORD-2024-001234",
                    OrderDate = "2024-01-15T10:30:00Z",
                    Status = "confirmed"
                },
                Customer = new Customer
                {
                    CustomerId = "CUST-5678",
                    Name = "Test Corp",
                    Email = "test@example.com",
                    Address = new Address
                    {
                        Street = "123 Test St",
                        City = "Helsinki",
                        PostalCode = "00100",
                        Country = "FI"
                    }
                },
                Items = new List<OrderItem>
                {
                    new()
                    {
                        LineNumber = 1,
                        ProductCode = "PROD-001",
                        Description = "Widget",
                        Quantity = 10,
                        UnitPrice = 29.99m,
                        Currency = "EUR"
                    }
                },
                Totals = new OrderTotals
                {
                    Subtotal = 299.90m,
                    TaxRate = 24m,
                    TaxAmount = 71.98m,
                    Total = 371.88m,
                    Currency = "EUR"
                }
            }
        }
    };

    [Fact]
    public void Transform_ValidBatch_ReturnsValidJson()
    {
        var batch = CreateTestBatch();

        var json = _sut.Transform(batch, new List<ValidationError>());

        var doc = JsonDocument.Parse(json);
        Assert.Equal("test-tenant", doc.RootElement.GetProperty("tenantId").GetString());
        Assert.Equal(1, doc.RootElement.GetProperty("orderCount").GetInt32());
        Assert.Equal(0, doc.RootElement.GetProperty("validationErrorCount").GetInt32());
    }

    [Fact]
    public void Transform_ValidBatch_IncludesOrderData()
    {
        var batch = CreateTestBatch();

        var json = _sut.Transform(batch, new List<ValidationError>());

        var doc = JsonDocument.Parse(json);
        var orders = doc.RootElement.GetProperty("orders");
        Assert.Equal(1, orders.GetArrayLength());

        var order = orders[0];
        Assert.Equal("ORD-2024-001234", order.GetProperty("header").GetProperty("orderId").GetString());
    }

    [Fact]
    public void Transform_WithValidationErrors_IncludesErrorsInOutput()
    {
        var batch = CreateTestBatch();
        var errors = new List<ValidationError>
        {
            new()
            {
                OrderId = "ORD-2024-001234",
                Field = "Customer.Email",
                Message = "Invalid email format",
                ErrorCode = "FORMAT"
            }
        };

        var json = _sut.Transform(batch, errors);

        var doc = JsonDocument.Parse(json);
        Assert.Equal(1, doc.RootElement.GetProperty("validationErrorCount").GetInt32());
        var validationErrors = doc.RootElement.GetProperty("validationErrors");
        Assert.Equal(1, validationErrors.GetArrayLength());
    }

    [Fact]
    public void Transform_NoValidationErrors_OmitsErrorsFromOutput()
    {
        var batch = CreateTestBatch();

        var json = _sut.Transform(batch, new List<ValidationError>());

        var doc = JsonDocument.Parse(json);
        Assert.False(doc.RootElement.TryGetProperty("validationErrors", out _));
    }
}
