namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatQualifiedAttributionInputModel
{
    public required IdentifierInputModel Agent { get; init; }

    public required CodeInputModel HadRole { get; init; }
}
