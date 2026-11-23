using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

// Keyed by the publisher identifier, or by its name when it has none; the key alone does not say which,
// so the identifier and name travel with the count.
// An internal publisher (an agent) always has both. An external publisher has at least one of the two:
// Identifier is null when only its name was entered, Name is null when only its identifier was entered.
public sealed record CatalogPublisherFacetCount
{
    public int Count { get; init; }

    public string? Identifier { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
