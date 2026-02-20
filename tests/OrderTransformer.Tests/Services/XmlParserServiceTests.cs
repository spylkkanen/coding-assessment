using Microsoft.Extensions.Logging.Abstractions;
using OrderTransformer.Services;
using Xunit;

namespace OrderTransformer.Tests.Services;

public class XmlParserServiceTests
{
    private readonly XmlParserService _sut = new(NullLogger<XmlParserService>.Instance);

    private static string LoadTestXml(string filename)
    {
        var path = Path.Combine("TestData", filename);
        return File.ReadAllText(path);
    }

    [Fact]
    public void Parse_ValidXml_ReturnsBatchWithCorrectTenantId()
    {
        var xml = LoadTestXml("valid-order.xml");

        var result = _sut.Parse(xml);

        Assert.Equal("test-tenant", result.TenantId);
    }

    [Fact]
    public void Parse_ValidXml_ParsesOrderHeader()
    {
        var xml = LoadTestXml("valid-order.xml");

        var result = _sut.Parse(xml);
        var order = result.Orders[0];

        Assert.Equal("ORD-2024-001234", order.Header.OrderId);
        Assert.Equal("2024-01-15T10:30:00Z", order.Header.OrderDate);
        Assert.Equal("confirmed", order.Header.Status);
    }

    [Fact]
    public void Parse_ValidXml_ParsesCustomerDetails()
    {
        var xml = LoadTestXml("valid-order.xml");

        var result = _sut.Parse(xml);
        var customer = result.Orders[0].Customer;

        Assert.Equal("CUST-5678", customer.CustomerId);
        Assert.Equal("Acme Corporation", customer.Name);
        Assert.Equal("orders@acme.example.com", customer.Email);
        Assert.Equal("FI", customer.Address.Country);
        Assert.Equal("Helsinki", customer.Address.City);
    }

    [Fact]
    public void Parse_ValidXml_ParsesItemsCorrectly()
    {
        var xml = LoadTestXml("valid-order.xml");

        var result = _sut.Parse(xml);
        var items = result.Orders[0].Items;

        Assert.Equal(2, items.Count);
        Assert.Equal("PROD-001", items[0].ProductCode);
        Assert.Equal(10, items[0].Quantity);
        Assert.Equal(29.99m, items[0].UnitPrice);
        Assert.Equal("EUR", items[0].Currency);
    }

    [Fact]
    public void Parse_ValidXml_ParsesTotals()
    {
        var xml = LoadTestXml("valid-order.xml");

        var result = _sut.Parse(xml);
        var totals = result.Orders[0].Totals;

        Assert.Equal(549.85m, totals.Subtotal);
        Assert.Equal(24m, totals.TaxRate);
        Assert.Equal(131.96m, totals.TaxAmount);
        Assert.Equal(681.81m, totals.Total);
    }

    [Fact]
    public void Parse_InvalidXml_ThrowsException()
    {
        var invalidXml = "this is not xml";

        Assert.Throws<System.Xml.XmlException>(() => _sut.Parse(invalidXml));
    }
}
