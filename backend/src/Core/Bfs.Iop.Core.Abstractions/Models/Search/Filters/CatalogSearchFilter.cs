using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models.Search.Filters;

public sealed record CatalogSearchFilter
{
    public IEnumerable<string> AccessRights { get; init; } = [];

    public IEnumerable<string> BusinessEvents { get; init; } = [];

    public IEnumerable<ConceptType> ConceptValueTypes {  get; init; } = [];
    
    public IEnumerable<string> Formats { get; init; } = [];

    public IEnumerable<string> LifeEvents { get; init; } = [];

    public IEnumerable<PublicationLevel> PublicationLevels { get; init; } = [];

    public IEnumerable<PublicationLevel> PublicationLevelProposals { get; init; } = [];

    public IEnumerable<string> PublisherIdentifiers { get; init; } = [];

    public IEnumerable<RegistrationStatus> RegistrationStatuses { get; init; } = [];

    public IEnumerable<RegistrationStatus> RegistrationStatusProposals { get; init; } = [];

    public SearchStructureOption? Structure { get; init; }

    public IEnumerable<string> Themes { get; init; } = [];

    public IEnumerable<SearchResourceType> Types { get; init; } = [];
}
