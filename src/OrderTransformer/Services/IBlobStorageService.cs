namespace OrderTransformer.Services;

public interface IBlobStorageService
{
    Task<List<string>> ListBlobsAsync(string prefix);
    Task<string> ReadBlobAsync(string blobName);
    Task WriteBlobAsync(string blobName, string content);
    Task MoveBlobAsync(string sourceBlobName, string destinationBlobName);
}
