using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CatalogSearchHit
{
    public required Guid Id { get; init; }

    public required SearchResourceType Type { get; init; }

    public string? Identifier { get; init; }

    public Guid PublisherId { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public DateTimeOffset? CreatedAt { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public CreationType CreationType { get; init; }

    public MultiLanguageModel? Title { get; init; }

    public MultiLanguageModel? Name { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public string? Version { get; init; }

    public IReadOnlyList<string> Themes { get; init; } = [];

    public string? AccessRights { get; init; }

    public IReadOnlyList<string> Formats { get; init; } = [];

    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    public ConceptType? ConceptType { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public bool? HasStructure { get; init; }
}
