using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using OrderTransformer.Models;

namespace OrderTransformer.Services;

public class JsonTransformerService : IJsonTransformerService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private readonly ILogger<JsonTransformerService> _logger;

    public JsonTransformerService(ILogger<JsonTransformerService> logger)
    {
        _logger = logger;
    }

    public string Transform(OrderBatch batch, List<ValidationError> validationErrors)
    {
        var output = new
        {
            batch.TenantId,
            ProcessedAt = DateTime.UtcNow.ToString("o"),
            OrderCount = batch.Orders.Count,
            ValidationErrorCount = validationErrors.Count,
            ValidationErrors = validationErrors.Count > 0 ? validationErrors : null,
            Orders = batch.Orders
        };

        var json = JsonSerializer.Serialize(output, JsonOptions);
        _logger.LogInformation("Transformed batch to JSON ({Length} bytes)", json.Length);
        return json;
    }
}
