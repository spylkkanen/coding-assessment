namespace OrderTransformer.Models;

public record OrderBatch
{
    public string TenantId { get; init; } = string.Empty;
    public List<Order> Orders { get; init; } = new();
}

public record Order
{
    public OrderHeader Header { get; init; } = new();
    public Customer Customer { get; init; } = new();
    public List<OrderItem> Items { get; init; } = new();
    public OrderTotals Totals { get; init; } = new();
}

public record OrderHeader
{
    public string OrderId { get; init; } = string.Empty;
    public string OrderDate { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
}

public record Customer
{
    public string CustomerId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public Address Address { get; init; } = new();
}

public record Address
{
    public string Street { get; init; } = string.Empty;
    public string City { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public string Country { get; init; } = string.Empty;
}

public record OrderItem
{
    public int LineNumber { get; init; }
    public string ProductCode { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal UnitPrice { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record OrderTotals
{
    public decimal Subtotal { get; init; }
    public decimal TaxRate { get; init; }
    public decimal TaxAmount { get; init; }
    public decimal Total { get; init; }
    public string Currency { get; init; } = string.Empty;
}
