namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogSearchResult
{
    public IReadOnlyList<CatalogSearchHit> Hits { get; init; } = [];

    public int Page { get; init; }

    public int PageSize { get; init; }

    public int TotalCount { get; init; }
}

public sealed record CatalogSearchHit
{
    public required Guid Id { get; init; }

    public required IndexResourceType Type { get; init; }

    public string? Identifier { get; init; }

    public Guid PublisherId { get; init; }

    public IndexPublicationLevel PublicationLevel { get; init; }

    public IndexPublicationLevel? PublicationLevelProposal { get; init; }

    public IndexRegistrationStatus RegistrationStatus { get; init; }

    public IndexRegistrationStatus? RegistrationStatusProposal { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public IndexCreationType CreationType { get; init; }

    public LocalizedText? Title { get; init; }

    public LocalizedText? Name { get; init; }

    public LocalizedText? Description { get; init; }

    public string? Version { get; init; }

    public IReadOnlyList<string> Themes { get; init; } = [];

    public string? AccessRights { get; init; }

    public IReadOnlyList<string> Formats { get; init; } = [];

    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    public IndexConceptType? ConceptType { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public bool? HasStructure { get; init; }
}
