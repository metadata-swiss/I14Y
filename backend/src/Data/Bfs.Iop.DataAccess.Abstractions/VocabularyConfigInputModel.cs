namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record VocabularyConfigInputModel
{
    public required string ConceptIdentifier { get; init; }

    public required string ConceptVersion { get; init; }

    public required string VocabularyIdentifier { get; init; }
}
