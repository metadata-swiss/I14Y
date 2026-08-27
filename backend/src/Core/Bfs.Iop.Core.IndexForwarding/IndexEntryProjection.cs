using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;

namespace Bfs.Iop.Core.IndexForwarding;

/// <summary>
/// Flattens Core's domain models into the index projection before they go on the wire.
/// <para>
/// The counterpart of the EF-side projection in <c>Bfs.Iop.Core.Data</c>: a rebuild projects from
/// entities, a single write projects from the model Core already holds. Both produce the same shape,
/// so the IndexSearch service has one document factory rather than two that must be kept in step.
/// </para>
/// <para>
/// Vocabulary-typed properties contribute only their <c>.Code</c>, because that is all the index
/// stores. The labels the model carries are dropped here rather than sent and discarded on the far
/// side.
/// </para>
/// </summary>
internal static class IndexEntryProjection
{
    public static CatalogIndexEntry FromDataset(DcatDatasetModel m) => Common(m, SearchResourceType.Dataset, m.Identifiers) with
    {
        Version = m.Version,
        AccessRights = m.AccessRights?.Code,
        Themes = [.. m.Themes.Select(x => x.Code)],
        Formats = [.. m.Distributions.Select(x => x.Format).Where(x => x is not null).Select(x => x!.Code).Distinct()],
        Title = m.Title,
        DataOwner = m.DataOwner,
        ResponsiblePerson = Person(m.ResponsiblePerson),
        ResponsibleDeputy = Person(m.ResponsibleDeputy),
        ContactPoints = ContactPoints(m.ContactPoints),
        // Left null: Core does not read the object store, so it cannot know. The IndexSearch service
        // resolves it, and null means "keep whatever is already indexed" rather than "false".
        HasStructure = null,
    };

    public static CatalogIndexEntry FromDataService(DataServiceModel m) => Common(m, SearchResourceType.DataService, m.Identifiers) with
    {
        Version = m.Version,
        AccessRights = m.AccessRights?.Code,
        Themes = [.. m.Themes.Select(x => x.Code)],
        Title = m.Title,
        ResponsiblePerson = Person(m.ResponsiblePerson),
        ResponsibleDeputy = Person(m.ResponsibleDeputy),
        ContactPoints = ContactPoints(m.ContactPoints),
    };

    public static CatalogIndexEntry FromPublicService(PublicServiceModel m) => Common(m, SearchResourceType.PublicService, m.Identifiers) with
    {
        // Two vocabularies feed the one themes field.
        Themes = [.. m.ThematicAreas.Concat(m.Sectors).Select(x => x.Code)],
        BusinessEvents = [.. m.BusinessEvents.Select(x => x.Code)],
        LifeEvents = [.. m.LifeEvents.Select(x => x.Code)],
        Title = m.Name,
        ResponsiblePerson = Person(m.ResponsiblePerson),
        ResponsibleDeputy = Person(m.ResponsibleDeputy),
        ChannelEmails = [.. m.Channels.Select(c => c.Email).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!)],
    };

    public static CatalogIndexEntry FromConcept(IopConceptModel m) => Common(m, SearchResourceType.Concept, m.Identifiers) with
    {
        Version = m.Version,
        Themes = [.. m.Themes.Select(x => x.Code)],
        // Concepts and mapping tables index their name twice: once as `name`, once as `title`.
        Name = m.Name,
        Title = m.Name,
        ConceptType = m.ConceptType,
        ValidFrom = m.ValidFrom,
        ValidTo = m.ValidTo,
        ResponsiblePerson = Person(m.ResponsiblePerson),
        ResponsibleDeputy = Person(m.ResponsibleDeputy),
    };

    public static CatalogIndexEntry FromMappingTable(MappingTableModel m) => Common(m, SearchResourceType.MappingTable, m.Identifiers) with
    {
        Version = m.Version,
        Themes = [.. m.Themes.Select(x => x.Code)],
        Name = m.Name,
        Title = m.Name,
        ValidFrom = m.ValidFrom,
        ValidTo = m.ValidTo,
        ResponsiblePerson = Person(m.ResponsiblePerson),
        ResponsibleDeputy = Person(m.ResponsibleDeputy),
    };

    public static CodeListIndexEntry FromCodeListEntry(CodeListEntryModel m) => new()
    {
        Id = m.Id,
        ConceptId = m.ConceptId,
        Code = m.Code,
        ParentCode = m.ParentCode,
        Name = m.Name,
        Description = m.Description,
        Annotations = [.. (m.Annotations ?? []).Select(a => new IndexAnnotation
        {
            Type = a.Type,
            Identifier = a.Identifier,
            Title = a.Title,
            Uri = a.Uri,
            Text = a.Text,
        })],
    };

    private static CatalogIndexEntry Common(IPublishableEntityModel m, SearchResourceType type, IEnumerable<string> identifiers) => new()
    {
        Id = m.Id,
        Type = type,
        Identifier = identifiers.FirstOrDefault() ?? string.Empty,
        PublisherId = m.Publisher.Id,
        // Case as stored: the document factory writes it both lowercased and case-preserved, and the
        // publishers facet depends on the case-preserved form surviving.
        PublisherIdentifier = m.Publisher.Identifier,
        PublicationLevel = m.PublicationLevel,
        PublicationLevelProposal = m.PublicationLevelProposal,
        RegistrationStatus = m.RegistrationStatus,
        RegistrationStatusProposal = m.RegistrationStatusProposal,
        // SystemInfoModel groups these on the model; the index stores them flat.
        CreatedAt = m.System.CreatedAt,
        ModifiedAt = m.System.ModifiedAt,
        CreationType = m.System.CreationType,
        Description = m.Description,
        Keywords = [.. m.Keywords.Where(x => x.Label is not null).Select(x => x.Label!)],
    };

    private static IndexPerson? Person(IopPersonModel? person) =>
        person is null ? null : new IndexPerson
        {
            Email = person.Email,
            GivenName = person.GivenName,
            FamilyName = person.FamilyName,
        };

    private static IReadOnlyList<IndexContactPoint> ContactPoints(IEnumerable<VCardModel>? contactPoints) =>
        contactPoints is null
            ? []
            : [.. contactPoints.Select(cp => new IndexContactPoint
            {
                Fn = cp.Fn,
                HasAddress = cp.HasAddress,
                Note = cp.Note,
                HasEmail = cp.HasEmail,
            })];
}
