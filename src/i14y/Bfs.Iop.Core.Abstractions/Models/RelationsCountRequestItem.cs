namespace Bfs.Iop.Core.Abstractions.Models;

/// <summary>
/// A single catalogue resource for which the relations-by count is requested. For concepts, the
/// concept IRI needed by the structure/mapping counts is resolved server-side from <see cref="Id"/>,
/// so no identifier/version need be supplied by the caller.
/// </summary>
public sealed record RelationsCountRequestItem
{
    public required Guid Id { get; init; }

    public required SearchResourceType Type { get; init; }
}
