namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record MappingRelationModel
{
    public Guid Id { get; init; }

    public required MappingRelationUriModel Source { get; init; }

    public required MappingRelationUriModel Target { get; init; }

    public required VocabularyEntryModel RelationType { get; init; }
}
