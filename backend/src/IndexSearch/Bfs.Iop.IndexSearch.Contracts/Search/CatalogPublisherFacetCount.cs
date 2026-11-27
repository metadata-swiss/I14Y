using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

// Keyed by the publisher identifier, or by "name:" and its name when it has none, so the two never collide.
// The identifier and name travel with the count so the key never has to be taken apart.
// An internal publisher (an agent) always has both. An external publisher has at least one of the two:
// Identifier is null when only its name was entered, Name is null when only its identifier was entered.
public sealed record CatalogPublisherFacetCount
{
    public int Count { get; init; }

    public string? Identifier { get; init; }

    public MultiLanguageModel? Name { get; init; }
}
