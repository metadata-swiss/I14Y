namespace Bfs.Iop.Infrastructure.FileStorage;

public sealed record StoredFileMetadata(
    string Filename,
    string? ContentType,
    long? ContentLength,
    DateTimeOffset? CreatedAt)
{ }
