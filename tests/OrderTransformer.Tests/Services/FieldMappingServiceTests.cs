using OrderTransformer.Models;
using OrderTransformer.Services;
using Xunit;

namespace OrderTransformer.Tests.Services;

public class FieldMappingServiceTests
{
    private readonly FieldMappingService _sut = new();

    private static OrderBatch CreateTestBatch(
        string country = "FI",
        string productCode = "PROD-001",
        string status = "confirmed") => new()
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
                    Status = status
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
                        Country = country
                    }
                },
                Items = new List<OrderItem>
                {
                    new()
                    {
                        LineNumber = 1,
                        ProductCode = productCode,
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
    public void MapFields_ValidBatch_ReturnsBatchWithSameOrderCount()
    {
        var batch = CreateTestBatch();

        var result = _sut.MapFields(batch);

        Assert.Equal(batch.Orders.Count, result.Orders.Count);
    }

    // --- Country code mappings ---

    [Theory]
    [InlineData("FI", "Finland")]
    [InlineData("SE", "Sweden")]
    [InlineData("NO", "Norway")]
    [InlineData("DK", "Denmark")]
    [InlineData("US", "United States")]
    [InlineData("GB", "United Kingdom")]
    [InlineData("DE", "Germany")]
    public void MapFields_CountryCode_MapsToFullName(string code, string expected)
    {
        var batch = CreateTestBatch(country: code);

        var result = _sut.MapFields(batch);

        Assert.Equal(expected, result.Orders[0].Customer.Address.Country);
    }

    [Fact]
    public void MapFields_UnknownCountryCode_LeavesUnchanged()
    {
        var batch = CreateTestBatch(country: "XX");

        var result = _sut.MapFields(batch);

        Assert.Equal("XX", result.Orders[0].Customer.Address.Country);
    }

    // --- Product code mappings ---

    [Theory]
    [InlineData("PROD-001", "Widgets")]
    [InlineData("PROD-002", "Gadgets")]
    [InlineData("PROD-003", "Premium Widgets")]
    public void MapFields_ProductCode_MapsToCategory(string productCode, string expectedCategory)
    {
        var batch = CreateTestBatch(productCode: productCode);

        var result = _sut.MapFields(batch);

        Assert.Contains(expectedCategory, result.Orders[0].Items[0].Description);
    }

    [Fact]
    public void MapFields_UnknownProductCode_LeavesDescriptionUnchanged()
    {
        var batch = CreateTestBatch(productCode: "PROD-999");

        var result = _sut.MapFields(batch);

        Assert.Equal("Widget", result.Orders[0].Items[0].Description);
    }

    // --- Status mappings ---

    [Theory]
    [InlineData("draft", "Draft")]
    [InlineData("confirmed", "Order Confirmed")]
    [InlineData("processing", "In Processing")]
    [InlineData("shipped", "Shipped")]
    [InlineData("delivered", "Delivered")]
    [InlineData("cancelled", "Cancelled")]
    public void MapFields_Status_MapsToDisplayLabel(string status, string expected)
    {
        var batch = CreateTestBatch(status: status);

        var result = _sut.MapFields(batch);

        Assert.Equal(expected, result.Orders[0].Header.Status);
    }

    [Fact]
    public void MapFields_UnknownStatus_LeavesUnchanged()
    {
        var batch = CreateTestBatch(status: "custom-status");

        var result = _sut.MapFields(batch);

        Assert.Equal("custom-status", result.Orders[0].Header.Status);
    }

    // --- Multiple orders ---

    [Fact]
    public void MapFields_MultipleOrders_AllAreMapped()
    {
        var batch = new OrderBatch
        {
            TenantId = "test-tenant",
            Orders = new List<Order>
            {
                new()
                {
                    Header = new OrderHeader { OrderId = "ORD-2024-000001", OrderDate = "2024-01-15T10:30:00Z", Status = "confirmed" },
                    Customer = new Customer
                    {
                        CustomerId = "CUST-001", Name = "Corp A", Email = "a@example.com",
                        Address = new Address { Street = "1 St", City = "Helsinki", PostalCode = "00100", Country = "FI" }
                    },
                    Items = new List<OrderItem> { new() { LineNumber = 1, ProductCode = "PROD-001", Description = "Widget", Quantity = 1, UnitPrice = 10m, Currency = "EUR" } },
                    Totals = new OrderTotals { Subtotal = 10m, TaxRate = 24m, TaxAmount = 2.40m, Total = 12.40m, Currency = "EUR" }
                },
                new()
                {
                    Header = new OrderHeader { OrderId = "ORD-2024-000002", OrderDate = "2024-01-16T10:30:00Z", Status = "shipped" },
                    Customer = new Customer
                    {
                        CustomerId = "CUST-002", Name = "Corp B", Email = "b@example.com",
                        Address = new Address { Street = "2 St", City = "Stockholm", PostalCode = "10000", Country = "SE" }
                    },
                    Items = new List<OrderItem> { new() { LineNumber = 1, ProductCode = "PROD-002", Description = "Gadget", Quantity = 5, UnitPrice = 20m, Currency = "USD" } },
                    Totals = new OrderTotals { Subtotal = 100m, TaxRate = 25m, TaxAmount = 25m, Total = 125m, Currency = "USD" }
                }
            }
        };

        var result = _sut.MapFields(batch);

        Assert.Equal("Finland", result.Orders[0].Customer.Address.Country);
        Assert.Equal("Order Confirmed", result.Orders[0].Header.Status);
        Assert.Equal("Sweden", result.Orders[1].Customer.Address.Country);
        Assert.Equal("Shipped", result.Orders[1].Header.Status);
    }
}
