using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record CatalogIndexDocument
{
    public required Guid Id { get; init; }

    public required SearchResourceType Type { get; init; }

    public string? Identifier { get; init; }

    public Guid PublisherId { get; init; }

    // Casing must be preserved: the index stores it lowercased for filtering and as-is for the facet.
    public string? PublisherIdentifier { get; init; }

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

    public IReadOnlyList<MultiLanguageModel> Keywords { get; init; } = [];

    public string? Version { get; init; }

    public string? DataOwner { get; init; }

    // Vocabulary fields hold codes, never labels.
    public string? AccessRights { get; init; }

    public IReadOnlyList<string> Themes { get; init; } = [];

    public IReadOnlyList<string> Formats { get; init; } = [];

    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    // Datasets only. null means "keep whatever is indexed", never false.
    public bool? HasStructure { get; init; }

    public ConceptType? ConceptType { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public IndexPerson? ResponsiblePerson { get; init; }

    public IndexPerson? ResponsibleDeputy { get; init; }

    public IReadOnlyList<IndexContactPoint> ContactPoints { get; init; } = [];

    public IReadOnlyList<string> ChannelEmails { get; init; } = [];
}

// Not IopPersonModel: that is a class, so a document holding one compares by reference and two
// documents with identical values are no longer equal.
public sealed record IndexPerson
{
    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }

    public string? Email { get; init; }
}

// Not VCardModel: its HasEmail is required, and MapToVCardModel maps a missing address to an empty
// string. The index drops absent e-mails by null, so "" would be indexed as a searchable term on
// every resource whose contact point has none.
public sealed record IndexContactPoint
{
    public MultiLanguageModel? Fn { get; init; }

    public MultiLanguageModel? HasAddress { get; init; }

    public MultiLanguageModel? Note { get; init; }

    public string? HasEmail { get; init; }
}
