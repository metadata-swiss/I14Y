using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;
using System.Net;

namespace Bfs.Iop.Core.FileStorage.Services;

internal sealed class S3FileStorageService : IFileStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3FileStorageService> _logger;

    public S3FileStorageService(IAmazonS3 s3Client, ILogger<S3FileStorageService> logger)
    {
        _s3Client = s3Client;
        _logger = logger;
    }

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

    /// <summary>
    /// Lists a bucket.
    /// <para>
    /// <b>ContentType is always null here</b>, unlike the Azure implementation. A key listing does not
    /// carry it, and filling it in would mean one HEAD request per object — fine for the handful of
    /// files in the media container, ruinous for the thousands in the dataset-structures one, which
    /// is this method's heaviest caller. Consumers already treat it as optional.
    /// </para>
    /// </summary>
    public async Task<IEnumerable<StoredFileMetadata>> GetContainerContentAsync(string container, CancellationToken cancellationToken = default)
    {
        var bucket = NormalizeContainerName(container);
        var request = new ListObjectsV2Request { BucketName = bucket };
        var files = new List<StoredFileMetadata>();

        try
        {
            ListObjectsV2Response response;

            do
            {
                response = await _s3Client.ListObjectsV2Async(request, cancellationToken);

                foreach (var item in response.S3Objects)
                {
                    files.Add(new StoredFileMetadata(item.Key, null, item.Size, item.LastModified));
                }

                // One call returns at most 1000 keys. Skipping the continuation token would make this
                // stop silently at 1000 — no error, no warning — and every dataset structure past
                // that point would look absent, so those datasets would be indexed as having none.
                request.ContinuationToken = response.NextContinuationToken;
            }
            while (response.IsTruncated == true);
        }
        catch (AmazonS3Exception e) when (e.StatusCode == HttpStatusCode.NotFound)
        {
            // An absent bucket is an empty listing, matching the Azure implementation, which creates
            // the container and then finds it empty. Throwing instead would turn "no media uploaded
            // yet" into a 500 on a fresh environment.
            //
            // Logged rather than silent, because the same answer covers a real misconfiguration: a
            // wrong bucket name reports an empty container just as convincingly as an empty one, and
            // downstream that surfaces only as a facet with no values.
            _logger.LogWarning(
                "Object store bucket '{Bucket}' does not exist; treating it as empty. If this is " +
                "unexpected, check the container name and the environment suffix.",
                bucket);

            return [];
        }

        return files;
    }

    public async Task<StoredFileMetadata> GetFileMetadataAsync(string container, string filename, CancellationToken cancellationToken = default)
    {
        var response = await _s3Client.GetObjectMetadataAsync(NormalizeContainerName(container), filename, cancellationToken);

        // The response carries no key, so the caller's filename is the name — the same thing the
        // Azure implementation does with blob.Name.
        return new StoredFileMetadata(
            filename,
            response.Headers.ContentType,
            response.ContentLength,
            response.LastModified);
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