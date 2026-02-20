using Microsoft.Extensions.Logging;
using OrderTransformer.Models;
using OrderTransformer.Services;

namespace OrderTransformer.Worker;

public class TransformationPipeline
{
    private readonly IXmlParserService _parser;
    private readonly IOrderValidatorService _validator;
    private readonly IFieldMappingService _mapper;
    private readonly IJsonTransformerService _transformer;
    private readonly ILogger<TransformationPipeline> _logger;

    public TransformationPipeline(
        IXmlParserService parser,
        IOrderValidatorService validator,
        IFieldMappingService mapper,
        IJsonTransformerService transformer,
        ILogger<TransformationPipeline> logger)
    {
        _parser = parser;
        _validator = validator;
        _mapper = mapper;
        _transformer = transformer;
        _logger = logger;
    }

    public TransformationResult Process(string xmlContent, string sourceBlobName)
    {
        try
        {
            _logger.LogInformation("Processing blob: {BlobName}", sourceBlobName);

            // Step 1: Parse XML to domain model
            var batch = _parser.Parse(xmlContent);
            _logger.LogInformation("Parsed {OrderCount} orders from {BlobName}", batch.Orders.Count, sourceBlobName);

            // Step 2: Validate orders
            var errors = _validator.Validate(batch);
            if (errors.Count > 0)
            {
                _logger.LogWarning("Validation found {ErrorCount} errors in {BlobName}", errors.Count, sourceBlobName);
            }

            // Step 3: Map field values
            var mappedBatch = _mapper.MapFields(batch);

            // Step 4: Transform to JSON
            var json = _transformer.Transform(mappedBatch, errors);

            _logger.LogInformation("Successfully processed {BlobName}", sourceBlobName);

            return new TransformationResult
            {
                Success = true,
                Json = json,
                ValidationErrors = errors,
                SourceBlobName = sourceBlobName
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process {BlobName}", sourceBlobName);
            return new TransformationResult
            {
                Success = false,
                SourceBlobName = sourceBlobName,
                ErrorMessage = ex.Message
            };
        }
    }
}
