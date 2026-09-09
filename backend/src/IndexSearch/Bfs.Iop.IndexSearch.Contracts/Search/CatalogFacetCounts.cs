namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogFacetCounts
{
    public IReadOnlyDictionary<string, int> Publishers { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Types { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Themes { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> AccessRights { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Formats { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> BusinessEvents { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> LifeEvents { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> ConceptTypes { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> PublicationLevels { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> PublicationLevelProposals { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> RegistrationStatuses { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> RegistrationStatusProposals { get; init; } = new Dictionary<string, int>();

    public IReadOnlyDictionary<string, int> Structures { get; init; } = new Dictionary<string, int>();

    public int TotalCount { get; init; }
}
