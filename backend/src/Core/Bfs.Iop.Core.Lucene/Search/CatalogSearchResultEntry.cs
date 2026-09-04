using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Lucene.Search;

public sealed record CatalogSearchResultEntry
{
    public string? AccessRights { get; init; }

    public DateTimeOffset CreatedAt { get; init; }

    public CreationType? CreationType { get; init; }

    public IEnumerable<string> BusinessEvents { get; init; } = [];

    public ConceptType? ConceptType { get; init; }

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<string> Formats { get; init; } = [];

    public bool? HasStructure { get; init; }

    public Guid Id { get; init; }

    public required string Identifier { get; init; }

    public IEnumerable<string> LifeEvents { get; init; } = [];

    public DateTimeOffset? ModifiedAt { get; init; }

    public PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public Guid Publisher { get; init; }

    public string? PublisherIdentifier { get; init; }

    public RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    public IEnumerable<string> Themes { get; init; } = [];

    public required MultiLanguageModel Title { get; init; }

    public required SearchResourceType Type { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public string? Version { get; init; }
}
