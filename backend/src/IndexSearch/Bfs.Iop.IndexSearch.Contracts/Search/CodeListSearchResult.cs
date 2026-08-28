namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CodeListSearchResult
{
    public IReadOnlyList<CodeListSearchHit> Hits { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}

public sealed record CodeListSearchHit
{
    public required Guid Id { get; init; }

    public required Guid ConceptId { get; init; }

    public required string Code { get; init; }

    public string? ParentCode { get; init; }

    public LocalizedText? Name { get; init; }

    public LocalizedText? Description { get; init; }

    public IReadOnlyList<CodeListAnnotationHit> Annotations { get; init; } = [];

    public IReadOnlyList<string> AncestorCodes { get; init; } = [];
}

public sealed record CodeListAnnotationHit
{
    public string? Type { get; init; }

    public string? Identifier { get; init; }

    public string? Title { get; init; }

    public string? Uri { get; init; }

    public LocalizedText? Text { get; init; }
}
