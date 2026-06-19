namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatQualifiedRelationInputModel
{
    public required CodeInputModel HadRole { get; init; }

    public required ResourceModel Relation { get; init; }
}
