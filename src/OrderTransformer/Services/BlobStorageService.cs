using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace OrderTransformer.Services;

public class BlobStorageService : IBlobStorageService
{
    private readonly BlobContainerClient _containerClient;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(IConfiguration configuration, ILogger<BlobStorageService> logger)
    {
        _logger = logger;
        var connectionString = configuration["BlobStorage:ConnectionString"]
            ?? throw new InvalidOperationException("BlobStorage:ConnectionString is not configured");
        var containerName = configuration["BlobStorage:ContainerName"] ?? "orders";

        _containerClient = new BlobContainerClient(connectionString, containerName);
    }

    public async Task<List<string>> ListBlobsAsync(string prefix)
    {
        var blobs = new List<string>();
        await foreach (BlobItem blob in _containerClient.GetBlobsAsync(prefix: prefix))
        {
            blobs.Add(blob.Name);
        }
        return blobs;
    }

    public async Task<string> ReadBlobAsync(string blobName)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        BlobDownloadResult result = await blobClient.DownloadContentAsync();
        return result.Content.ToString();
    }

    public async Task WriteBlobAsync(string blobName, string content)
    {
        var blobClient = _containerClient.GetBlobClient(blobName);
        await blobClient.UploadAsync(BinaryData.FromString(content), overwrite: true);
        _logger.LogInformation("Written blob: {BlobName}", blobName);
    }

    public async Task MoveBlobAsync(string sourceBlobName, string destinationBlobName)
    {
        var sourceClient = _containerClient.GetBlobClient(sourceBlobName);
        var destinationClient = _containerClient.GetBlobClient(destinationBlobName);

        await destinationClient.StartCopyFromUriAsync(sourceClient.Uri);
        await sourceClient.DeleteAsync();
        _logger.LogInformation("Moved blob: {Source} -> {Destination}", sourceBlobName, destinationBlobName);
    }
}
