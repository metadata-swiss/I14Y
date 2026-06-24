namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record SystemInfoModel
{
    public DateTimeOffset CreatedAt { get; init; }

    public CreationType? CreationType { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }
}
