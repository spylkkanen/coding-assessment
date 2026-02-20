using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using OrderTransformer.Services;

namespace OrderTransformer.Worker;

public class BlobPollingWorker : BackgroundService
{
    private readonly IBlobStorageService _blobService;
    private readonly TransformationPipeline _pipeline;
    private readonly ILogger<BlobPollingWorker> _logger;
    private readonly string _inputPrefix;
    private readonly string _outputPrefix;
    private readonly string _processedPrefix;
    private readonly string _failedPrefix;
    private readonly int _pollingIntervalSeconds;
    private readonly HashSet<string> _processingBlobs = new();

    public BlobPollingWorker(
        IBlobStorageService blobService,
        TransformationPipeline pipeline,
        IConfiguration configuration,
        ILogger<BlobPollingWorker> logger)
    {
        _blobService = blobService;
        _pipeline = pipeline;
        _logger = logger;
        _inputPrefix = configuration["BlobStorage:InputPrefix"] ?? "input/";
        _outputPrefix = configuration["BlobStorage:OutputPrefix"] ?? "output/";
        _processedPrefix = configuration["BlobStorage:ProcessedPrefix"] ?? "processed/";
        _failedPrefix = configuration["BlobStorage:FailedPrefix"] ?? "failed/";
        _pollingIntervalSeconds = int.TryParse(configuration["BlobStorage:PollingIntervalSeconds"], out var interval) ? interval : 5;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Blob polling worker started. Polling every {Interval}s for prefix '{Prefix}'",
            _pollingIntervalSeconds, _inputPrefix);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await PollAndProcessAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during polling cycle");
            }

            await Task.Delay(TimeSpan.FromSeconds(_pollingIntervalSeconds), stoppingToken);
        }

        _logger.LogInformation("Blob polling worker stopped");
    }

    private async Task PollAndProcessAsync(CancellationToken stoppingToken)
    {
        var blobs = await _blobService.ListBlobsAsync(_inputPrefix);

        foreach (var blobName in blobs)
        {
            if (stoppingToken.IsCancellationRequested) break;
            if (_processingBlobs.Contains(blobName)) continue;

            _processingBlobs.Add(blobName);

            try
            {
                _logger.LogInformation("Found new blob: {BlobName}", blobName);

                var xmlContent = await _blobService.ReadBlobAsync(blobName);
                var result = _pipeline.Process(xmlContent, blobName);

                var fileName = Path.GetFileNameWithoutExtension(blobName.Replace(_inputPrefix, ""));

                if (result.Success)
                {
                    var outputBlobName = $"{_outputPrefix}{fileName}.json";
                    await _blobService.WriteBlobAsync(outputBlobName, result.Json);
                    await _blobService.MoveBlobAsync(blobName, $"{_processedPrefix}{Path.GetFileName(blobName)}");
                    _logger.LogInformation("Processed {BlobName} -> {OutputBlob}", blobName, outputBlobName);
                }
                else
                {
                    await _blobService.MoveBlobAsync(blobName, $"{_failedPrefix}{Path.GetFileName(blobName)}");
                    _logger.LogWarning("Failed to process {BlobName}: {Error}", blobName, result.ErrorMessage);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing blob {BlobName}", blobName);
            }
            finally
            {
                _processingBlobs.Remove(blobName);
            }
        }
    }
}
