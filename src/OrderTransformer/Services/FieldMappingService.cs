using OrderTransformer.Models;

namespace OrderTransformer.Services;

public class FieldMappingService : IFieldMappingService
{
    private static readonly Dictionary<string, string> CountryMap = new()
    {
        ["FI"] = "Finland",
        ["SE"] = "Sweden",
        ["NO"] = "Norway",
        ["DK"] = "Denmark",
        ["US"] = "United States",
        ["GB"] = "United Kingdom",
        ["DE"] = "Germany"
    };

    private static readonly Dictionary<string, string> ProductCategoryMap = new()
    {
        ["PROD-001"] = "Widgets",
        ["PROD-002"] = "Gadgets",
        ["PROD-003"] = "Premium Widgets"
    };

    private static readonly Dictionary<string, string> StatusMap = new()
    {
        ["draft"] = "Draft",
        ["confirmed"] = "Order Confirmed",
        ["processing"] = "In Processing",
        ["shipped"] = "Shipped",
        ["delivered"] = "Delivered",
        ["cancelled"] = "Cancelled"
    };

    public OrderBatch MapFields(OrderBatch batch)
    {
        var mappedOrders = batch.Orders.Select(MapOrder).ToList();
        return batch with { Orders = mappedOrders };
    }

    private static Order MapOrder(Order order)
    {
        var mappedCountry = CountryMap.TryGetValue(order.Customer.Address.Country, out var countryName)
            ? countryName
            : order.Customer.Address.Country;

        var mappedStatus = StatusMap.TryGetValue(order.Header.Status, out var statusLabel)
            ? statusLabel
            : order.Header.Status;

        var mappedItems = order.Items.Select(MapItem).ToList();

        return order with
        {
            Header = order.Header with { Status = mappedStatus },
            Customer = order.Customer with
            {
                Address = order.Customer.Address with { Country = mappedCountry }
            },
            Items = mappedItems
        };
    }

    private static OrderItem MapItem(OrderItem item)
    {
        if (ProductCategoryMap.TryGetValue(item.ProductCode, out var category))
        {
            return item with { Description = category };
        }
        return item;
    }
}
