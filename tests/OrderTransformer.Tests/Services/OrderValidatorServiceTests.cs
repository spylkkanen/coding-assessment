using OrderTransformer.Models;
using OrderTransformer.Services;
using Xunit;

namespace OrderTransformer.Tests.Services;

public class OrderValidatorServiceTests
{
    private readonly OrderValidatorService _sut = new();

    private static OrderBatch CreateValidBatch() => new()
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

    // --- Valid batch ---

    [Fact]
    public void Validate_ValidOrderBatch_ReturnsNoErrors()
    {
        var batch = CreateValidBatch();

        var errors = _sut.Validate(batch);

        Assert.Empty(errors);
    }

    // --- Required field validations ---

    [Fact]
    public void Validate_MissingOrderId_ReturnsRequiredError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Header = order.Header with { OrderId = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Header.OrderId" && e.ErrorCode == "REQUIRED");
    }

    [Fact]
    public void Validate_MissingOrderDate_ReturnsRequiredError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Header = order.Header with { OrderDate = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Header.OrderDate" && e.ErrorCode == "REQUIRED");
    }

    [Fact]
    public void Validate_MissingCustomerId_ReturnsRequiredError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Customer = order.Customer with { CustomerId = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Customer.CustomerId" && e.ErrorCode == "REQUIRED");
    }

    [Fact]
    public void Validate_MissingCustomerName_ReturnsRequiredError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Customer = order.Customer with { Name = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Customer.Name" && e.ErrorCode == "REQUIRED");
    }

    [Fact]
    public void Validate_MissingCustomerEmail_ReturnsRequiredError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Customer = order.Customer with { Email = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Customer.Email" && e.ErrorCode == "REQUIRED");
    }

    [Fact]
    public void Validate_MissingOrderId_UsesUnknownAsOrderId()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Header = order.Header with { OrderId = "" } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.OrderId == "unknown" && e.Field == "Header.OrderId");
    }

    // --- Format validations: OrderId ---

    [Theory]
    [InlineData("INVALID-ID")]
    [InlineData("ORD-24-001234")]
    [InlineData("ORD-2024-12345")]
    [InlineData("ord-2024-001234")]
    [InlineData("ORD-2024-0012345")]
    public void Validate_InvalidOrderIdFormat_ReturnsFormatError(string orderId)
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Header = order.Header with { OrderId = orderId } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Header.OrderId" && e.ErrorCode == "FORMAT");
    }

    // --- Format validations: Email ---

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@missing.com")]
    [InlineData("missing@")]
    [InlineData("missing@domain")]
    public void Validate_InvalidEmail_ReturnsFormatError(string email)
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with { Orders = new List<Order> { order with { Customer = order.Customer with { Email = email } } } };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Customer.Email" && e.ErrorCode == "FORMAT");
    }

    // --- Format validations: Country code ---

    [Theory]
    [InlineData("finland")]
    [InlineData("FIN")]
    [InlineData("f")]
    [InlineData("fi")]
    public void Validate_InvalidCountryCode_ReturnsFormatError(string country)
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with { Customer = order.Customer with { Address = order.Customer.Address with { Country = country } } }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Customer.Address.Country" && e.ErrorCode == "FORMAT");
    }

    // --- Format validations: Currency code ---

    [Theory]
    [InlineData("euro")]
    [InlineData("EU")]
    [InlineData("euros")]
    [InlineData("eur")]
    public void Validate_InvalidCurrencyCode_ReturnsFormatError(string currency)
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with
                {
                    Items = new List<OrderItem>
                    {
                        order.Items[0] with { Currency = currency }
                    }
                }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Items[0].Currency" && e.ErrorCode == "FORMAT");
    }

    // --- Range validations: Quantity ---

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_ZeroOrNegativeQuantity_ReturnsRangeError(int quantity)
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with
                {
                    Items = new List<OrderItem>
                    {
                        order.Items[0] with { Quantity = quantity }
                    }
                }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Items[0].Quantity" && e.ErrorCode == "RANGE");
    }

    // --- Range validations: UnitPrice ---

    [Fact]
    public void Validate_NegativeUnitPrice_ReturnsRangeError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with
                {
                    Items = new List<OrderItem>
                    {
                        order.Items[0] with { UnitPrice = -1.00m }
                    }
                }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Items[0].UnitPrice" && e.ErrorCode == "RANGE");
    }

    [Fact]
    public void Validate_ZeroUnitPrice_ReturnsNoRangeError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with
                {
                    Items = new List<OrderItem>
                    {
                        order.Items[0] with { UnitPrice = 0m }
                    }
                }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.DoesNotContain(errors, e => e.Field == "Items[0].UnitPrice" && e.ErrorCode == "RANGE");
    }

    // --- Range validations: Totals ---

    [Fact]
    public void Validate_NegativeSubtotal_ReturnsRangeError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with { Totals = order.Totals with { Subtotal = -1m } }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Totals.Subtotal" && e.ErrorCode == "RANGE");
    }

    [Fact]
    public void Validate_NegativeTaxAmount_ReturnsRangeError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with { Totals = order.Totals with { TaxAmount = -1m } }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Totals.TaxAmount" && e.ErrorCode == "RANGE");
    }

    [Fact]
    public void Validate_NegativeTotal_ReturnsRangeError()
    {
        var batch = CreateValidBatch();
        var order = batch.Orders[0];
        batch = batch with
        {
            Orders = new List<Order>
            {
                order with { Totals = order.Totals with { Total = -1m } }
            }
        };

        var errors = _sut.Validate(batch);

        Assert.Contains(errors, e => e.Field == "Totals.Total" && e.ErrorCode == "RANGE");
    }
}
