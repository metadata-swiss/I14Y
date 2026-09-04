namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatQualifiedAttributionModel
{
    public AgentModel Agent { get; init; } = null!;

    public VocabularyEntryModel HadRole { get; init; } = null!;
}
