namespace OrderTransformer.Models;

public record TransformationResult
{
    public bool Success { get; init; }
    public string Json { get; init; } = string.Empty;
    public List<ValidationError> ValidationErrors { get; init; } = new();
    public string SourceBlobName { get; init; } = string.Empty;
    public string? ErrorMessage { get; init; }
}
