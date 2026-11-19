namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatQualifiedAttributionModel
{
    public IdentifierNameModel Agent { get; init; } = null!;

    public VocabularyEntryModel HadRole { get; init; } = null!;
}
