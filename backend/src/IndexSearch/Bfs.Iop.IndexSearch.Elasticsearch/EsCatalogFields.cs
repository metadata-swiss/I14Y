using Bfs.Iop.IndexSearch.Contracts;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class EsCatalogFields
{
    public static readonly IReadOnlyList<string> Languages = LocalizedText.Languages;

    public const string Title = "title";
    public const string Name = "name";
    public const string Description = "description";
    public const string Keyword = "keyword";
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
}
