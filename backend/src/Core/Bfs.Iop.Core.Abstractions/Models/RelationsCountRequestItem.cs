using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record RelationsCountRequestItem
{
    public required Guid Id { get; init; }

    public required SearchResourceType Type { get; init; }
}
