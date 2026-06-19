namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatQualifiedAttributionModel
{
    public AgentModel Agent { get; init; } = null!;

    public VocabularyEntryModel HadRole { get; init; } = null!;
}
