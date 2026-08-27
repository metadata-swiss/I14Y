using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Data.Indexing;

/// <summary>
/// Flattens entities into the index projection.
/// <para>
/// Vocabulary-coded columns are copied across as the raw strings they already are. The index stores
/// codes and never labels, so there is nothing to resolve — the previous route through the domain
/// models turned each code into a labelled object only for the document factory to read
/// <c>.Code</c> back off it.
/// </para>
/// <para>
/// Every field here is one the index actually stores. If a property is missing from an entry, the
/// document simply lacks that field: no error, no warning, just results that quietly do not match.
/// Check against <c>CatalogDocumentFactory</c> before removing anything.
/// </para>
/// </summary>
internal static class IndexEntryFactory
{
    public static CatalogIndexEntry FromDataset(Dataset e) => Common(e, SearchResourceType.Dataset, e.Identifier) with
    {
        Version = e.Version,
        AccessRights = e.AccessRights,
        Themes = e.Theme,
        Formats = [.. e.Distributions.Select(x => x.Format).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!).Distinct()],
        Title = Lang(e.Title),
        Description = Lang(e.Description),
        DataOwner = e.DataOwner,
        Keywords = Keywords(e.Keyword),
        ResponsiblePerson = Person(e.ResponsiblePerson),
        ResponsibleDeputy = Person(e.ResponsibleDeputy),
        ContactPoints = ContactPoints(e.ContactPoint),
        // Left null on purpose: it comes from the object store, not this database. The caller fills
        // it in, and null means "keep whatever is already indexed".
        HasStructure = null,
    };

    public static CatalogIndexEntry FromDataService(DataService e) => Common(e, SearchResourceType.DataService, e.Identifiers) with
    {
        Version = e.Version,
        AccessRights = e.AccessRights,
        Themes = e.Theme,
        Title = Lang(e.Title),
        Description = Lang(e.Description),
        Keywords = Keywords(e.Keyword),
        ResponsiblePerson = Person(e.ResponsiblePerson),
        ResponsibleDeputy = Person(e.ResponsibleDeputy),
        ContactPoints = ContactPoints(e.ContactPoint),
    };

    public static CatalogIndexEntry FromPublicService(PublicService e) => Common(e, SearchResourceType.PublicService, e.Identifiers) with
    {
        // Two vocabularies feed the one themes field, matching what the document factory did with
        // ThematicAreas and Sectors.
        Themes = [.. e.ThematicArea.Concat(e.Sector)],
        BusinessEvents = e.BusinessEvents,
        LifeEvents = e.LifeEvents,
        // The entity calls it Title; it is the public service's name, and it is indexed as the title.
        Title = Lang(e.Title),
        Description = Lang(e.Description),
        Keywords = Keywords(e.Keyword),
        ResponsiblePerson = Person(e.ResponsiblePerson),
        ResponsibleDeputy = Person(e.ResponsibleDeputy),
        ChannelEmails = [.. e.Channels.Select(c => c.Email).Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x!)],
    };

    public static CatalogIndexEntry FromConcept(IopConcept e) => Common(e, SearchResourceType.Concept, e.Identifiers) with
    {
        Version = e.Version,
        Themes = [.. e.Themes],
        // Concepts and mapping tables index their name twice: once as `name`, once as `title`.
        Name = Lang(e.Name),
        Title = Lang(e.Name),
        Description = Lang(e.Description),
        ConceptType = e.ConceptType,
        ValidFrom = e.ValidFrom,
        ValidTo = e.ValidTo,
        Keywords = Keywords(e.Keywords),
        ResponsiblePerson = Person(e.ResponsiblePerson),
        ResponsibleDeputy = Person(e.ResponsibleDeputy),
    };

    public static CatalogIndexEntry FromMappingTable(MappingTable e) => Common(e, SearchResourceType.MappingTable, e.Identifiers) with
    {
        Version = e.Version,
        Themes = e.Themes,
        Name = Lang(e.Name),
        Title = Lang(e.Name),
        Description = Lang(e.Description),
        ValidFrom = e.ValidFrom,
        ValidTo = e.ValidTo,
        Keywords = Keywords(e.Keywords),
        ResponsiblePerson = Person(e.ResponsiblePerson),
        ResponsibleDeputy = Person(e.ResponsibleDeputy),
    };

    public static CodeListIndexEntry FromCodeListEntry(CodeListEntry e) => new()
    {
        Id = e.Id,
        ConceptId = e.IopConceptId,
        Code = e.Code,
        // The parent's CODE, not its id. Requires ParentCodeListEntry to have been included — if it
        // was not, this silently flattens the hierarchy rather than failing.
        ParentCode = e.ParentCodeListEntry?.Code,
        Name = Lang(e.Name) ?? new MultiLanguageModel(),
        Description = Lang(e.Description),
        Annotations = [.. e.Annotations.Select(a => new IndexAnnotation
        {
            Type = a.Type,
            Identifier = a.Identifier,
            Title = a.Title,
            Uri = a.Uri,
            Text = Lang(a.Text),
        })],
    };

    private static CatalogIndexEntry Common(PublishableEntityBase e, SearchResourceType type, IEnumerable<string> identifiers) => new()
    {
        Id = e.Id,
        Type = type,
        // The index stores one identifier; resources carry a list. Matches the document factory,
        // which took Identifiers.First().
        Identifier = identifiers.FirstOrDefault() ?? string.Empty,
        PublisherId = e.PublisherId,
        // Case as stored — the document factory writes it both lowercased and case-preserved.
        PublisherIdentifier = e.Publisher.Identifier,
        PublicationLevel = e.PublicationLevel,
        PublicationLevelProposal = e.PublicationLevelProposal,
        RegistrationStatus = e.RegistrationStatus,
        RegistrationStatusProposal = e.RegistrationStatusProposal,
        CreatedAt = e.CreatedAt,
        ModifiedAt = e.ModifiedAt,
        CreationType = e.CreationType,
    };

    private static IReadOnlyList<MultiLanguageModel> Keywords(IEnumerable<Keyword> keywords) =>
        [.. keywords.Select(k => Lang(k.Text)).Where(x => x is not null).Select(x => x!)];

    private static IndexPerson? Person(IopPerson? person) =>
        person is null ? null : new IndexPerson
        {
            Email = person.Email,
            GivenName = person.GivenName,
            FamilyName = person.FamilyName,
        };

    private static IReadOnlyList<IndexContactPoint> ContactPoints(IEnumerable<VCard> contactPoints) =>
        [.. contactPoints.Select(cp => new IndexContactPoint
        {
            Fn = Lang(cp.Fn),
            // The entity's column names differ from the indexed field names: AdrWork is the address,
            // EmailInternet is the e-mail.
            HasAddress = Lang(cp.AdrWork),
            Note = Lang(cp.Note),
            HasEmail = cp.EmailInternet,
        })];

    private static MultiLanguageModel? Lang(MultiLanguage? value) =>
        value is null ? null : new MultiLanguageModel
        {
            De = value.De,
            En = value.En,
            Fr = value.Fr,
            It = value.It,
            Rm = value.Rm,
        };
}
