namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record CodeListIndexDocument
{
    public required Guid Id { get; init; }

    public required Guid ConceptId { get; init; }

    public required string Code { get; init; }

    public string? ParentCode { get; init; }

    public LocalizedText? Name { get; init; }

    public LocalizedText? Description { get; init; }

    public IReadOnlyList<IndexAnnotation> Annotations { get; init; } = [];
}

public sealed record IndexAnnotation
{
    public string? Type { get; init; }

    public string? Identifier { get; init; }

    public string? Title { get; init; }

    public string? Uri { get; init; }

    public LocalizedText? Text { get; init; }
}
