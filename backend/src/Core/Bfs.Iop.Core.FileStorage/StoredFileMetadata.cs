namespace Bfs.Iop.Core.FileStorage;

public sealed record StoredFileMetadata(
    string Filename,
    string? ContentType,
    long? ContentLength,
    DateTimeOffset? CreatedAt)
{ }
