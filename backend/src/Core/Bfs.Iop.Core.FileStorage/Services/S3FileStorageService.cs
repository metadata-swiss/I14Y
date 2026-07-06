using Amazon.S3;
using Amazon.S3.Model;
using System.Net;

namespace Bfs.Iop.Core.FileStorage.Services;

internal sealed class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;

    public S3FileStorageService(IAmazonS3 s3Client) => _s3Client = s3Client;

    public Task DeleteAsync(string container, string filename, CancellationToken cancellationToken = default)
        => _s3Client.DeleteObjectAsync(NormalizeContainerName(container), filename, cancellationToken);

    public async Task<StoredFile> DownloadAsync(string container, string filename, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectAsync(NormalizeContainerName(container), filename, cancellationToken);

        var metadata = new StoredFileMetadata(
            filename, 
            response.Headers.ContentType, 
            response.ContentLength,
            response.LastModified);

        return new(response.ResponseStream, metadata);
    }

    public async Task<bool> ExistsAsync(string container, string filename, CancellationToken cancellationToken = default)
    {
        try
        {
            await _s3Client.GetObjectMetadataAsync(NormalizeContainerName(container), filename, cancellationToken);

            return true;
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }
    }

    public async Task UploadAsync(string container, string filename, Stream content, string contentType, CancellationToken cancellationToken = default)
    {
        await EnsureContainerExists(NormalizeContainerName(container), cancellationToken);

        var request = new PutObjectRequest
        {
            BucketName = NormalizeContainerName(container),
            Key = filename,
            InputStream = content,
            ContentType = contentType
        };

        await _s3Client.PutObjectAsync(request, cancellationToken);
    }

    public Task<IEnumerable<StoredFileMetadata>> GetContainerContentAsync(string container, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task<StoredFileMetadata> GetFileMetadataAsync(string container, string filename, CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    private async Task EnsureContainerExists(string container, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.ListBucketsAsync(cancellationToken);

        if (!response.Buckets.Any(b => b.BucketName.Equals(NormalizeContainerName(container), StringComparison.OrdinalIgnoreCase)))
        {
            await _s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = NormalizeContainerName(container),
                UseClientRegion = true
            }, cancellationToken);
        }
    }

    private static string NormalizeContainerName(string container)
        => container.ToLowerInvariant();
}