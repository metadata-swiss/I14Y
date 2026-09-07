using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Indexing;

public sealed record CatalogIndexDocument
{
    public required Guid Id { get; init; }

    public required SearchResourceType Type { get; init; }

    public IReadOnlyList<string> Identifiers { get; init; } = [];

    public Guid PublisherId { get; init; }

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

    // IopPersonModel is a class, not a record, so these two compare by reference and a document
    // holding one is not value-equal to an identical document. Round-trip assertions must compare
    // the fields rather than the documents.
    public IopPersonModel? ResponsiblePerson { get; init; }

    public IopPersonModel? ResponsibleDeputy { get; init; }

    public IReadOnlyList<VCardModel> ContactPoints { get; init; } = [];

    public IReadOnlyList<string> ChannelEmails { get; init; } = [];
}
