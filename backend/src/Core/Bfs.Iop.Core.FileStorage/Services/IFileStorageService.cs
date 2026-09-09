namespace Bfs.Iop.Core.FileStorage.Services;

public interface IFileStorageService
{
    Task UploadAsync(string container, string filename, Stream content, string contentType, CancellationToken cancellationToken = default);

    Task<StoredFile> DownloadAsync(string container, string filename, CancellationToken cancellationToken = default);

    Task<bool> ExistsAsync(string container, string filename, CancellationToken cancellationToken = default);

    Task DeleteAsync(string container, string filename, CancellationToken cancellationToken = default);

    Task<IEnumerable<StoredFileMetadata>> GetContainerContentAsync(string container, CancellationToken cancellationToken = default);

    Task<StoredFileMetadata> GetFileMetadataAsync(string container, string filename, CancellationToken cancellationToken = default);
}