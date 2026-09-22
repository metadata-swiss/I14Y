namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record MappingRelationModel
{
    public Guid Id { get; init; }

    public required MappingRelationUriModel Source { get; init; }

    public required MappingRelationUriModel Target { get; init; }

    public required VocabularyEntryModel RelationType { get; init; }
}
