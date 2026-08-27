namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Field names used in the Elasticsearch catalog index. Kept in one place so the mapping,
/// the document factory and the query builder always agree. Multilingual fields are objects
/// with per-language sub-fields, addressed as e.g. <c>title.de</c>.
/// </summary>
internal static class EsCatalogFields
{
    public static readonly string[] Languages = ["de", "en", "fr", "it", "rm"];

    // Multilingual (object with de/en/fr/it/rm sub-fields, each with a .ngram sub-field for partial match).
    public const string Title = "title";
    public const string Name = "name";
    public const string Description = "description";
    public const string Keyword = "keyword";
    public const string ContactPointFn = "contactPointFn";
    public const string ContactPointHasAddress = "contactPointHasAddress";
    public const string ContactPointNote = "contactPointNote";

    public static readonly string[] MultiLanguageFields =
    [
        Title, Name, Description, Keyword, ContactPointFn, ContactPointHasAddress, ContactPointNote
    ];

    // Single-value / keyword / numeric / date fields.
    public const string Id = "id";
    public const string Identifier = "identifier";
    public const string DataOwner = "dataOwner";
    public const string Version = "version";
    public const string Publisher = "publisher";
    /// <summary>
    /// The publisher identifier, <b>lowercased</b>. Term-matching only: the authorization clause and
    /// the publishers filter both lowercase their input before querying it.
    /// </summary>
    public const string PublisherIdentifier = "publisherIdentifier";

    /// <summary>
    /// The publisher identifier in its <b>original case</b>, for the publishers facet only.
    /// <para>
    /// Two fields rather than one, and the split is load-bearing: facet bucket
    /// keys are fed to <c>IAgentReader.GetAgents(IEnumerable&lt;string&gt;)</c>, which resolves them
    /// with a case-sensitive <c>= ANY(…)</c> against <c>agents.identifier</c>. Identifiers are stored
    /// as entered and are uppercase by convention (CH_BFS, CH_MIN), so bucketing on the lowercased
    /// field returns keys that match no agent row — and every publisher bucket is then dropped,
    /// leaving the facet silently empty.
    /// </para>
    /// </summary>
    public const string PublisherIdentifierLabel = "publisherIdentifierLabel";
    public const string RegistrationStatus = "registrationStatus";
    public const string RegistrationStatusProposal = "registrationStatusProposal";
    public const string RegistrationStatusWeight = "registrationStatusWeight";
    public const string PublicationLevel = "publicationLevel";
    public const string PublicationLevelProposal = "publicationLevelProposal";
    public const string ConceptType = "conceptType";
    public const string Type = "type";
    public const string Themes = "themes";
    public const string AccessRights = "accessRights";
    public const string BusinessEvents = "businessEvents";
    public const string LifeEvents = "lifeEvents";
    public const string Formats = "formats";
    public const string HasStructure = "hasStructure";
    public const string ContactPointHasEmail = "contactPointHasEmail";
    public const string ResponsiblePersonEmail = "responsiblePersonEmail";
    public const string ResponsibleDeputyEmail = "responsibleDeputyEmail";
    public const string ResponsiblePersonName = "responsiblePersonName";
    public const string ResponsibleDeputyName = "responsibleDeputyName";
    public const string CreatedAt = "createdAt";
    public const string CreationType = "creationType";
    public const string ModifiedAt = "modifiedAt";
    public const string ValidFrom = "validFrom";
    public const string ValidTo = "validTo";

    // Email fields, indexed as a single exact (keyword) token — see the email special-case (bug #725).
    public static readonly string[] EmailFields =
    [
        ResponsiblePersonEmail, ResponsibleDeputyEmail, ContactPointHasEmail
    ];
}
