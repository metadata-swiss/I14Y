namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatQualifiedAttributionInputModel
{
    public required IdentifierNameInputModel Agent { get; init; }

    public required CodeInputModel HadRole { get; init; }
}
