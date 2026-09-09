namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatQualifiedRelationModel
{
    public VocabularyEntryModel HadRole { get; init; } = null!;

    public ResourceModel Relation { get; init; } = null!;
}
