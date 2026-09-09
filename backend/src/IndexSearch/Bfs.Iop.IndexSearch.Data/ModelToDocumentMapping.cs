using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Data;

internal static class ModelToDocumentMapping
{
    public static CatalogIndexDocument ToIndexDocument(this DcatDatasetModel model) => new CatalogIndexDocument
    {
        Id = model.Id,
        Type = SearchResourceType.Dataset,
        Identifiers = [.. model.Identifiers],
        Title = model.Title.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Keywords = model.Keywords.ToTexts(),
        Version = model.Version,
        AccessRights = model.AccessRights?.Code,
        Themes = model.Themes.Codes(),
        DataOwner = model.DataOwner,
        Formats = [.. model.Distributions.Select(x => x.Format?.Code).OfType<string>().Distinct()],
        ResponsiblePerson = model.ResponsiblePerson.ToIndexPerson(),
        ResponsibleDeputy = model.ResponsibleDeputy.ToIndexPerson(),
        ContactPoints = model.ContactPoints.ToContactPoints(),
        HasStructure = null,
    }.WithPublishableFields(model.Publisher, model.System, model.PublicationLevel,
        model.PublicationLevelProposal, model.RegistrationStatus, model.RegistrationStatusProposal);

    public static CatalogIndexDocument ToIndexDocument(this DataServiceModel model) => new CatalogIndexDocument
    {
        Id = model.Id,
        Type = SearchResourceType.DataService,
        Identifiers = [.. model.Identifiers],
        Title = model.Title.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Keywords = model.Keywords.ToTexts(),
        Version = model.Version,
        AccessRights = model.AccessRights?.Code,
        Themes = model.Themes.Codes(),
        ResponsiblePerson = model.ResponsiblePerson.ToIndexPerson(),
        ResponsibleDeputy = model.ResponsibleDeputy.ToIndexPerson(),
        ContactPoints = model.ContactPoints.ToContactPoints(),
    }.WithPublishableFields(model.Publisher, model.System, model.PublicationLevel,
        model.PublicationLevelProposal, model.RegistrationStatus, model.RegistrationStatusProposal);

    public static CatalogIndexDocument ToIndexDocument(this PublicServiceModel model) => new CatalogIndexDocument
    {
        Id = model.Id,
        Type = SearchResourceType.PublicService,
        Identifiers = [.. model.Identifiers],
        Name = model.Name.NullIfEmpty(),
        Title = model.Name.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Keywords = model.Keywords.ToTexts(),
        Themes = [.. model.ThematicAreas.Concat(model.Sectors).Select(x => x.Code)],
        BusinessEvents = model.BusinessEvents.Codes(),
        LifeEvents = model.LifeEvents.Codes(),
        ResponsiblePerson = model.ResponsiblePerson.ToIndexPerson(),
        ResponsibleDeputy = model.ResponsibleDeputy.ToIndexPerson(),
        ChannelEmails = [.. model.Channels
            .Select(x => x.Email?.Trim())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x!)],
    }.WithPublishableFields(model.Publisher, model.System, model.PublicationLevel,
        model.PublicationLevelProposal, model.RegistrationStatus, model.RegistrationStatusProposal);

    public static CatalogIndexDocument ToIndexDocument(this IopConceptModel model) => new CatalogIndexDocument
    {
        Id = model.Id,
        Type = SearchResourceType.Concept,
        Identifiers = [.. model.Identifiers],
        Name = model.Name.NullIfEmpty(),
        Title = model.Name.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Keywords = model.Keywords.ToTexts(),
        Version = model.Version,
        Themes = model.Themes.Codes(),
        ConceptType = model.ConceptType,
        ValidFrom = model.ValidFrom,
        ValidTo = model.ValidTo,
        ResponsiblePerson = model.ResponsiblePerson.ToIndexPerson(),
        ResponsibleDeputy = model.ResponsibleDeputy.ToIndexPerson(),
    }.WithPublishableFields(model.Publisher, model.System, model.PublicationLevel,
        model.PublicationLevelProposal, model.RegistrationStatus, model.RegistrationStatusProposal);

    public static CatalogIndexDocument ToIndexDocument(this MappingTableModel model) => new CatalogIndexDocument
    {
        Id = model.Id,
        Type = SearchResourceType.MappingTable,
        Identifiers = [.. model.Identifiers],
        Name = model.Name.NullIfEmpty(),
        Title = model.Name.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Keywords = model.Keywords.ToTexts(),
        Version = model.Version,
        Themes = model.Themes.Codes(),
        ValidFrom = model.ValidFrom,
        ValidTo = model.ValidTo,
        ResponsiblePerson = model.ResponsiblePerson.ToIndexPerson(),
        ResponsibleDeputy = model.ResponsibleDeputy.ToIndexPerson(),
    }.WithPublishableFields(model.Publisher, model.System, model.PublicationLevel,
        model.PublicationLevelProposal, model.RegistrationStatus, model.RegistrationStatusProposal);

    public static CodeListIndexDocument ToIndexDocument(
        this CodeListEntryModel model,
        IReadOnlyList<string> ancestorCodes) => new()
    {
        Id = model.Id,
        ConceptId = model.ConceptId,
        Code = model.Code,
        ParentCode = model.ParentCode,
        AncestorCodes = ancestorCodes,
        Name = model.Name.NullIfEmpty(),
        Description = model.Description.NullIfEmpty(),
        Annotations = [.. (model.Annotations ?? []).Select(x => new AnnotationInputModel
        {
            Type = x.Type,
            Identifier = x.Identifier,
            Title = x.Title,
            Uri = x.Uri,
            Text = x.Text.NullIfEmpty(),
        })],
    };

    private static CatalogIndexDocument WithPublishableFields(
        this CatalogIndexDocument document,
        AgentModel? publisher,
        SystemInfoModel system,
        PublicationLevel publicationLevel,
        PublicationLevel? publicationLevelProposal,
        RegistrationStatus registrationStatus,
        RegistrationStatus? registrationStatusProposal) =>
        document with
        {
            PublisherId = publisher?.Id ?? Guid.Empty,
            PublisherIdentifier = publisher?.Identifier,
            PublicationLevel = publicationLevel,
            PublicationLevelProposal = publicationLevelProposal,
            RegistrationStatus = registrationStatus,
            RegistrationStatusProposal = registrationStatusProposal,
            CreatedAt = system.CreatedAt,
            ModifiedAt = system.ModifiedAt,
            CreationType = system.CreationType ?? CreationType.Manual,
        };

    private static IndexPerson? ToIndexPerson(this IopPersonModel? person) =>
        person is null
            ? null
            : new IndexPerson
            {
                GivenName = person.GivenName,
                FamilyName = person.FamilyName,
                Email = person.Email,
            };


    private static IReadOnlyList<IndexContactPoint> ToContactPoints(this IEnumerable<VCardModel> contactPoints) =>
        [.. contactPoints.Select(x => new IndexContactPoint
        {
            Fn = x.Fn.NullIfEmpty(),
            HasAddress = x.HasAddress.NullIfEmpty(),
            Note = x.Note.NullIfEmpty(),
            HasEmail = string.IsNullOrWhiteSpace(x.HasEmail) ? null : x.HasEmail,
            HasTelephone = x.HasTelephone,
        })];

    private static IReadOnlyList<string> Codes(this IEnumerable<VocabularyEntryModel> entries) =>
        [.. entries.Select(x => x.Code).Where(x => !string.IsNullOrWhiteSpace(x))];

    private static IReadOnlyList<MultiLanguageModel> ToTexts(this IEnumerable<KeywordModel> keywords) =>
        [.. keywords.Select(x => x.Label.NullIfEmpty()).OfType<MultiLanguageModel>()];

    private static MultiLanguageModel? NullIfEmpty(this MultiLanguageModel? value) =>
        value is null || value.IsContentNullOrWhiteSpace() ? null : value;
}