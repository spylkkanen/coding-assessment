using System.Xml.Linq;
using Microsoft.Extensions.Logging;
using OrderTransformer.Models;

namespace OrderTransformer.Services;

public class XmlParserService : IXmlParserService
{
    private static readonly XNamespace Ns = "http://example.com/schemas/order/v1";
    private readonly ILogger<XmlParserService> _logger;

    public XmlParserService(ILogger<XmlParserService> logger)
    {
        _logger = logger;
    }

    public OrderBatch Parse(string xml)
    {
        var doc = XDocument.Parse(xml);
        var root = doc.Root ?? throw new InvalidOperationException("XML document has no root element");

        var tenantId = root.Element(Ns + "tenantId")?.Value ?? string.Empty;
        var orders = root.Elements(Ns + "order").Select(ParseOrder).ToList();

        _logger.LogInformation("Parsed {OrderCount} orders for tenant {TenantId}", orders.Count, tenantId);

        return new OrderBatch
        {
            TenantId = tenantId,
            Orders = orders
        };
    }

    private Order ParseOrder(XElement orderElement)
    {
        var header = ParseHeader(orderElement.Element(Ns + "header")!);
        var customer = ParseCustomer(orderElement.Element(Ns + "customer")!);
        var items = orderElement.Element(Ns + "items")!
            .Elements(Ns + "item")
            .Select(ParseItem)
            .ToList();
        var totals = ParseTotals(orderElement.Element(Ns + "totals")!);

        return new Order
        {
            Header = header,
            Customer = customer,
            Items = items,
            Totals = totals
        };
    }

    private OrderHeader ParseHeader(XElement element) => new()
    {
        OrderId = element.Element(Ns + "orderId")?.Value ?? string.Empty,
        OrderDate = element.Element(Ns + "orderDate")?.Value ?? string.Empty,
        Status = element.Element(Ns + "status")?.Value ?? string.Empty
    };

    private Customer ParseCustomer(XElement element) => new()
    {
        CustomerId = element.Element(Ns + "customerId")?.Value ?? string.Empty,
        Name = element.Element(Ns + "name")?.Value ?? string.Empty,
        Email = element.Element(Ns + "email")?.Value ?? string.Empty,
        Address = ParseAddress(element.Element(Ns + "address")!)
    };

    private Address ParseAddress(XElement element) => new()
    {
        Street = element.Element(Ns + "street")?.Value ?? string.Empty,
        City = element.Element(Ns + "city")?.Value ?? string.Empty,
        PostalCode = element.Element(Ns + "postalCode")?.Value ?? string.Empty,
        Country = element.Element(Ns + "country")?.Value ?? string.Empty
    };

    private OrderItem ParseItem(XElement element) => new()
    {
        LineNumber = int.TryParse(element.Element(Ns + "lineNumber")?.Value, out var ln) ? ln : 0,
        ProductCode = element.Element(Ns + "productCode")?.Value ?? string.Empty,
        Description = element.Element(Ns + "description")?.Value ?? string.Empty,
        Quantity = int.TryParse(element.Element(Ns + "quantity")?.Value, out var qty) ? qty : 0,
        UnitPrice = decimal.TryParse(element.Element(Ns + "unitPrice")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var price) ? price : 0,
        Currency = element.Element(Ns + "currency")?.Value ?? string.Empty
    };

    private OrderTotals ParseTotals(XElement element) => new()
    {
        Subtotal = decimal.TryParse(element.Element(Ns + "subtotal")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var sub) ? sub : 0,
        TaxRate = decimal.TryParse(element.Element(Ns + "taxRate")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var rate) ? rate : 0,
        TaxAmount = decimal.TryParse(element.Element(Ns + "taxAmount")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var tax) ? tax : 0,
        Total = decimal.TryParse(element.Element(Ns + "total")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out var total) ? total : 0,
        Currency = element.Element(Ns + "currency")?.Value ?? string.Empty
    };
}
