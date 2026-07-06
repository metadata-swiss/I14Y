namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record VocabularyConfigModel
{
    public Guid Id { get; init; }

    public required string ConceptIdentifier { get; init; }

    public required string ConceptVersion { get; init; }

    public required string VocabularyIdentifier { get; init; }
}
