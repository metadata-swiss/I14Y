using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace Bfs.Iop.Core.FileStorage.Services;

internal sealed class AzureFileStorageService : IFileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;

    public AzureFileStorageService(BlobServiceClient blobServiceClient) => _blobServiceClient = blobServiceClient;

    public Task DeleteAsync(string container, string filename, CancellationToken cancellationToken = default)
        => GetBlob(container, filename)
           .DeleteIfExistsAsync(DeleteSnapshotsOption.IncludeSnapshots, cancellationToken: cancellationToken);

    public async Task<StoredFile> DownloadAsync(string container, string filename, CancellationToken cancellationToken = default)
    {
        var blob = GetBlob(container, filename);
        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

        var stream = await blob.OpenReadAsync(cancellationToken: cancellationToken);
        var metadata = new StoredFileMetadata(
            blob.Name,
            properties.Value.ContentType,
            properties.Value.ContentLength,
            properties.Value.CreatedOn);

        return new StoredFile(stream, metadata);
    }

    public async Task<bool> ExistsAsync(string container, string filename, CancellationToken cancellationToken = default)
        => (await GetBlob(container, filename).ExistsAsync(cancellationToken)).Value;

    public async Task UploadAsync(string container, string filename, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        content.Position = 0; // set the position to start to comply easier with Azure

        var blobContainerClient = GetBlobContainerClient(container);

        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobClient = blobContainerClient.GetBlobClient(filename);

        // simplest reliable path: overwrite + then set headers
        await blobClient.UploadAsync(content, overwrite: true, cancellationToken: cancellationToken);

        if (!string.IsNullOrWhiteSpace(contentType))
            await blobClient.SetHttpHeadersAsync(new BlobHttpHeaders { ContentType = contentType }, cancellationToken: cancellationToken);
    }

    public async Task<IEnumerable<StoredFileMetadata>> GetContainerContentAsync(string container, CancellationToken cancellationToken = default)
    {
        var blobContainerClient = GetBlobContainerClient(container);

        await blobContainerClient.CreateIfNotExistsAsync(cancellationToken: cancellationToken);

        var blobs = blobContainerClient.GetBlobs(BlobTraits.Metadata, cancellationToken: cancellationToken);

        var infos = blobs
            .Where(x => !x.Deleted)
            .Select(x => new StoredFileMetadata(
                x.Name, 
                x.Properties.ContentType,
                x.Properties.ContentLength, 
                x.Properties.CreatedOn));

        return infos;
    }

    public async Task<StoredFileMetadata> GetFileMetadataAsync(
        string container,
        string filename,
        CancellationToken cancellationToken = default)
    {
        var blob = GetBlob(container, filename);
        var properties = await blob.GetPropertiesAsync(cancellationToken: cancellationToken);

        return new StoredFileMetadata(
            blob.Name,
            properties.Value.ContentType,
            properties.Value.ContentLength,
            properties.Value.CreatedOn);
    }

    private static string Normalize(string name) => name.ToLowerInvariant();

    private BlobContainerClient GetBlobContainerClient(string container)
        => _blobServiceClient.GetBlobContainerClient(Normalize(container));

    private BlobClient GetBlob(string container, string filename)
        => GetBlobContainerClient(container).GetBlobClient(filename);
}
