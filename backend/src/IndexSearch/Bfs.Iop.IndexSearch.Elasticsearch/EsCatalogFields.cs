using Bfs.Iop.IndexSearch.Contracts;
using System.Globalization;
using System.Security;
using static Microsoft.Extensions.Logging.EventSource.LoggingEventSource;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class EsCatalogFields
{
    public static readonly IReadOnlyList<string> Languages = IndexLanguages.All;

    public const string Title = "title";
    public const string Name = "name";
    public const string Description = "description";
    public const string Keyword = "keyword";

    // Exact, case-insensitive copy of the keywords of every language, used only for filtering.
    // Distinct from Keyword, the per-language analysed text used for free-text search.
    public const string KeywordFilter = "keywordFilter";
    public const string ContactPointFn = "contactPointFn";
    public const string ContactPointHasAddress = "contactPointHasAddress";
    public const string ContactPointNote = "contactPointNote";

    public static readonly IReadOnlyList<string> MultiLanguageFields =
    [
        Title, Name, Description, Keyword, ContactPointFn, ContactPointHasAddress, ContactPointNote,
    ];

    public const string Id = "id";
    public const string Identifier = "identifier";
    public const string DataOwner = "dataOwner";
    public const string Version = "version";
    public const string Publisher = "publisher";
    public const string PublisherIdentifier = "publisherIdentifier";
    public const string PublisherIdentifierLabel = "publisherIdentifierLabel";
    public const string RegistrationStatus = "registrationStatus";
    public const string RegistrationStatusProposal = "registrationStatusProposal";
    public const string RegistrationStatusWeight = "registrationStatusWeight";
    public const string PublicationLevel = "publicationLevel";
    public const string PublicationLevelProposal = "publicationLevelProposal";
    public const string ConceptType = "conceptType";
    public const string Type = "type";
    public const string Themes = "themes";
    public const string AttributedAgentIdentifiers = "attributedAgentIdentifiers";
    public const string AccessRights = "accessRights";
    public const string BusinessEvents = "businessEvents";
    public const string LifeEvents = "lifeEvents";
    public const string Formats = "formats";
    public const string Licenses = "licenses";
    public const string HasStructure = "hasStructure";
    public const string ReuseCount = "reuseCount";
    public const string ContactPointHasEmail = "contactPointHasEmail";
    public const string ContactPointHasTelephone = "contactPointHasTelephone";
    public const string ResponsiblePersonEmail = "responsiblePersonEmail";
    public const string ResponsibleDeputyEmail = "responsibleDeputyEmail";
    public const string ResponsiblePersonName = "responsiblePersonName";
    public const string ResponsibleDeputyName = "responsibleDeputyName";
    public const string ChannelEmail = "channelEmail";
    public const string CreatedAt = "createdAt";
    public const string CreationType = "creationType";
    public const string ModifiedAt = "modifiedAt";
    public const string ValidFrom = "validFrom";
    public const string ValidTo = "validTo";

    public static readonly IReadOnlyList<string> EmailFields =
    [
        ResponsiblePersonEmail, ResponsibleDeputyEmail, ContactPointHasEmail, ChannelEmail,
    ];

    public static readonly IReadOnlyList<string> NonPublicFields =
    [
        DataOwner, ResponsiblePersonName, ResponsibleDeputyName, ResponsiblePersonEmail, ResponsibleDeputyEmail,
    ];

    public static readonly IReadOnlyList<string> SearchableKeywordFields =
    [
        Identifier, DataOwner, Version, ResponsiblePersonName, ResponsibleDeputyName, ContactPointHasTelephone,
    ];

    public static string TextOf(string field) => $"{field}.{EsAnalysis.TextSubField}";

    /// <summary>
    ///     How much each field counts towards relevance. What a resource is called, and the
    ///     identifiers people paste to find one exact thing, decide the match; everything else only
    ///     separates resources whose names matched equally well.
    /// </summary>
    private static readonly IReadOnlyDictionary<string, double> _weights = new Dictionary<string, double>
    {
        [Title] = 5,
        [Name] = 5,
        [Identifier] = 5,
        [Version] = 5,
        [Keyword] = 1,
        [Description] = 0.3,
        [ContactPointFn] = 0.2,
        [ContactPointHasAddress] = 0.2,
        [ContactPointNote] = 0.2,
        [ContactPointHasTelephone] = 0.2,
        [DataOwner] = 0.2,
        [ResponsiblePersonName] = 0.2,
        [ResponsibleDeputyName] = 0.2,
    };

    /// <summary>
    ///     Appends the field's weight to a path such as "title.de" or "identifier.text", leaving a
    ///     field nobody weighted at Elasticsearch's own default of 1.
    /// </summary>
    public static string Weighted(string field, string path) =>
        _weights.TryGetValue(field, out var weight)
            // Invariant, or a Swiss locale writes 0,3 and Elasticsearch rejects the field.
            ? $"{path}^{weight.ToString(CultureInfo.InvariantCulture)}"
            : path;
}
