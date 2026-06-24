namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MediaInfoModel
{
    public long? ContentLength { get; init; }

    public string? ContentType { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public required string Filename { get; init; }

    public string? Url { get; init; }
}
