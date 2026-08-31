namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogSearchFilter
{
    public IReadOnlyList<string> AccessRights { get; init; } = [];

    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    public IReadOnlyList<IndexConceptType> ConceptTypes { get; init; } = [];

    public IReadOnlyList<string> Formats { get; init; } = [];

    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    public IReadOnlyList<IndexPublicationLevel> PublicationLevels { get; init; } = [];

    public IReadOnlyList<IndexPublicationLevel> PublicationLevelProposals { get; init; } = [];

    public IReadOnlyList<string> PublisherIdentifiers { get; init; } = [];

    public IReadOnlyList<IndexRegistrationStatus> RegistrationStatuses { get; init; } = [];

    public IReadOnlyList<IndexRegistrationStatus> RegistrationStatusProposals { get; init; } = [];

    public IndexStructureOption? Structure { get; init; }

    public IReadOnlyList<string> Themes { get; init; } = [];

    public IReadOnlyList<IndexResourceType> Types { get; init; } = [];
}
