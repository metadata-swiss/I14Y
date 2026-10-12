namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogSearchRequest
{
    public string? Query { get; init; }

    public string? Language { get; init; }

    public CatalogSearchFilter Filter { get; init; } = new();

    public int Page { get; init; } = SearchPaging.DefaultPage;

    public int PageSize { get; init; } = SearchPaging.DefaultPageSize;
}

public sealed record CatalogFacetRequest
{
    public string? Query { get; init; }

    public string? Language { get; init; }

    public CatalogSearchFilter Filter { get; init; } = new();
}

public sealed record CodeListSearchRequest
{
    public Guid ConceptId { get; init; }

    public string? Query { get; init; }

    public string? Language { get; init; }

    public CodeListSearchFilter Filter { get; init; } = new();

    public int Page { get; init; } = SearchPaging.DefaultPage;

    public int PageSize { get; init; } = SearchPaging.DefaultPageSize;
}
