using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogFacetLabelledCount
{
    public int Count { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
