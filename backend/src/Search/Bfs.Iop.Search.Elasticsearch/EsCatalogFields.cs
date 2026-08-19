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
    public const string PublisherIdentifier = "publisherIdentifier";
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
