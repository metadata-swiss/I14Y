namespace Bfs.Iop.Core.Abstractions.Models.Indexing;

/// <summary>
/// A catalog resource flattened into exactly what the search index stores — and nothing else.
/// <para>
/// This is the single input to the index document factory, produced two ways: projected from EF in
/// <c>Bfs.Iop.Core.Data</c> for a full rebuild, and projected from a domain model in
/// <c>Bfs.Iop.Core.IndexForwarding</c> for a single write. It is also the wire contract of
/// <c>POST /api/Index/*</c>.
/// </para>
/// <para>
/// <b>Vocabulary fields are raw codes, deliberately.</b> The index stores codes and never labels —
/// <c>themes</c>, <c>accessRights</c>, <c>businessEvents</c>, <c>lifeEvents</c> and <c>formats</c> are
/// mapped as plain Elasticsearch <c>keyword</c> fields with no analyzer, so they cannot hold
/// searchable text. Labels are resolved on the way out, per request, from the vocabulary tables.
/// Resolving them here would be work whose result is immediately discarded.
/// </para>
/// <para>
/// The type is deliberately flat: no navigation properties, so it cannot carry an EF object graph, and
/// projecting it runs as a single SQL <c>SELECT</c> rather than a fan-out of <c>Include</c>s.
/// </para>
/// </summary>
public sealed record CatalogIndexEntry
{
    public required Guid Id { get; init; }

    /// <summary>Which of the five catalog resource kinds this is; indexed as its name.</summary>
    public required SearchResourceType Type { get; init; }

    /// <summary>The first identifier. Resources carry a list; the index stores one.</summary>
    public required string Identifier { get; init; }

    public required Guid PublisherId { get; init; }

    /// <summary>
    /// Case as stored. The document factory writes it twice — lowercased for term matching and
    /// case-preserved for the publishers facet — so this must not be normalised on the way in.
    /// </summary>
    public required string PublisherIdentifier { get; init; }

    public required PublicationLevel PublicationLevel { get; init; }

    public PublicationLevel? PublicationLevelProposal { get; init; }

    public required RegistrationStatus RegistrationStatus { get; init; }

    public RegistrationStatus? RegistrationStatusProposal { get; init; }

    /// <summary>Flattened from <c>SystemInfoModel</c>, which groups these three on the domain model.</summary>
    public required DateTimeOffset CreatedAt { get; init; }

    public DateTimeOffset? ModifiedAt { get; init; }

    public CreationType? CreationType { get; init; }

    /// <summary>Datasets and data services use their title; the other three use their name.</summary>
    public MultiLanguageModel? Title { get; init; }

    /// <summary>Concepts and mapping tables only; indexed separately from <see cref="Title"/>.</summary>
    public MultiLanguageModel? Name { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public IReadOnlyList<MultiLanguageModel> Keywords { get; init; } = [];

    public string? Version { get; init; }

    /// <summary>Vocabulary code, not a label.</summary>
    public string? AccessRights { get; init; }

    /// <summary>
    /// Vocabulary codes. For a public service this is thematic areas and sectors merged — the index
    /// has one themes field and both vocabularies feed it.
    /// </summary>
    public IReadOnlyList<string> Themes { get; init; } = [];

    /// <summary>Distinct distribution format codes. Datasets only.</summary>
    public IReadOnlyList<string> Formats { get; init; } = [];

    /// <summary>Public services only.</summary>
    public IReadOnlyList<string> BusinessEvents { get; init; } = [];

    /// <summary>Public services only.</summary>
    public IReadOnlyList<string> LifeEvents { get; init; } = [];

    /// <summary>Datasets only.</summary>
    public string? DataOwner { get; init; }

    /// <summary>
    /// Datasets only, and resolved by the IndexSearch service rather than carried from Core: it comes
    /// from the object store, not the database. Null means "leave whatever is already indexed".
    /// </summary>
    public bool? HasStructure { get; init; }

    /// <summary>Concepts only.</summary>
    public ConceptType? ConceptType { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }

    public IndexPerson? ResponsiblePerson { get; init; }

    public IndexPerson? ResponsibleDeputy { get; init; }

    public IReadOnlyList<IndexContactPoint> ContactPoints { get; init; } = [];

    /// <summary>
    /// Public-service channel e-mail addresses. Indexed into the same field as contact-point e-mails,
    /// because a user searching for an address does not know which of the two it came from.
    /// </summary>
    public IReadOnlyList<string> ChannelEmails { get; init; } = [];
}

/// <summary>The three fields of a responsible person the index actually stores.</summary>
public sealed record IndexPerson
{
    public required string Email { get; init; }

    public string? GivenName { get; init; }

    public string? FamilyName { get; init; }
}

/// <summary>The four contact-point fields the index actually stores.</summary>
public sealed record IndexContactPoint
{
    public MultiLanguageModel? Fn { get; init; }

    public MultiLanguageModel? HasAddress { get; init; }

    public MultiLanguageModel? Note { get; init; }

    public string? HasEmail { get; init; }
}
