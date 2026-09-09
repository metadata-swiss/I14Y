namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record ConceptReferenceModel
{
    public required string Uri { get; init; }

    public MultiLanguageModel? Name { get; init; }

    public Guid? ConceptId { get; init; }
}
