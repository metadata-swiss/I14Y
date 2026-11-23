namespace Bfs.Iop.Infrastructure.FileStorage;

public sealed record StoredFile(
    Stream Stream,
    StoredFileMetadata Metadata) : IDisposable
{
    public void Dispose() => Stream.Dispose();
}
