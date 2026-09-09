using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogSearchFilter
{
    public IReadOnlyList<string> AccessRights { get; init; } = [];

    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    public IReadOnlyList<ConceptType> ConceptTypes { get; init; } = [];

    public IReadOnlyList<string> Formats { get; init; } = [];

    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    public IReadOnlyList<PublicationLevel> PublicationLevels { get; init; } = [];

    public IReadOnlyList<PublicationLevel> PublicationLevelProposals { get; init; } = [];

    public IReadOnlyList<string> PublisherIdentifiers { get; init; } = [];

    public IReadOnlyList<RegistrationStatus> RegistrationStatuses { get; init; } = [];

    public IReadOnlyList<RegistrationStatus> RegistrationStatusProposals { get; init; } = [];

    public IndexStructureOption? Structure { get; init; }

    public IReadOnlyList<string> Themes { get; init; } = [];

    public IReadOnlyList<SearchResourceType> Types { get; init; } = [];
}
