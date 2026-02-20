namespace OrderTransformer.Models;

public record ValidationError
{
    public string OrderId { get; init; } = string.Empty;
    public string Field { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public string ErrorCode { get; init; } = string.Empty;
}
