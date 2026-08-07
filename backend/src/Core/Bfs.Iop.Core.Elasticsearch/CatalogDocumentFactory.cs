using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Elasticsearch;

/// <summary>
/// Builds the JSON document (as a dictionary) indexed for each catalog resource. Mirrors the
/// field set and boost-relevant content of the Lucene <c>CatalogIndexService.BuildDocument</c>,
/// but the boosts themselves are applied at query time in <see cref="CatalogQueryBuilder"/>
/// (the Elasticsearch idiom) rather than at index time.
/// </summary>
internal static class CatalogDocumentFactory
{
    public static (string Id, Dictionary<string, object?> Document) FromDataset(DcatDatasetModel dataset, bool hasStructure)
    {
        var doc = BuildCommon(dataset, SearchResourceType.Dataset, dataset.Identifiers.First());

        doc[EsCatalogFields.AccessRights] = dataset.AccessRights?.Code;
        SetIfNotNull(doc, EsCatalogFields.Version, dataset.Version);
        doc[EsCatalogFields.Themes] = dataset.Themes.Select(x => x.Code).ToArray();
        doc[EsCatalogFields.Formats] = dataset.Distributions
            .Select(x => x.Format)
            .Where(x => x is not null)
            .Select(x => x!.Code)
            .Distinct()
            .ToArray();
        SetSingleMultiLang(doc, EsCatalogFields.Title, dataset.Title);

        if (!string.IsNullOrWhiteSpace(dataset.DataOwner))
        {
            doc[EsCatalogFields.DataOwner] = dataset.DataOwner.Trim().ToLowerInvariant();
        }

        AddPeople(doc, dataset.ResponsiblePerson, dataset.ResponsibleDeputy);
        AddContactPoints(doc, dataset.ContactPoints);
        doc[EsCatalogFields.HasStructure] = hasStructure;

        return (GuidToString(dataset.Id), doc);
    }

    public static (string Id, Dictionary<string, object?> Document) FromDataService(DataServiceModel model)
    {
        var doc = BuildCommon(model, SearchResourceType.DataService, model.Identifiers.First());

        SetIfNotNull(doc, EsCatalogFields.Version, model.Version);
        doc[EsCatalogFields.AccessRights] = model.AccessRights?.Code;
        doc[EsCatalogFields.Themes] = model.Themes.Select(x => x.Code).ToArray();
        SetSingleMultiLang(doc, EsCatalogFields.Title, model.Title);
        AddPeople(doc, model.ResponsiblePerson, model.ResponsibleDeputy);
        AddContactPoints(doc, model.ContactPoints);

        return (GuidToString(model.Id), doc);
    }

    public static (string Id, Dictionary<string, object?> Document) FromPublicService(PublicServiceModel model)
    {
        var doc = BuildCommon(model, SearchResourceType.PublicService, model.Identifiers.First());

        doc[EsCatalogFields.Themes] = model.ThematicAreas.Concat(model.Sectors).Select(x => x.Code).ToArray();
        SetSingleMultiLang(doc, EsCatalogFields.Title, model.Name);
        doc[EsCatalogFields.BusinessEvents] = model.BusinessEvents.Select(x => x.Code).ToArray();
        doc[EsCatalogFields.LifeEvents] = model.LifeEvents.Select(x => x.Code).ToArray();
        AddPeople(doc, model.ResponsiblePerson, model.ResponsibleDeputy);

        var emails = model.Channels
            .Select(c => c.Email?.Trim())
            .Where(e => !string.IsNullOrWhiteSpace(e))
            .Select(e => e!.ToLowerInvariant())
            .ToArray();
        if (emails.Length > 0)
        {
            doc[EsCatalogFields.ContactPointHasEmail] = emails;
        }

        return (GuidToString(model.Id), doc);
    }

    public static (string Id, Dictionary<string, object?> Document) FromConcept(IopConceptModel model)
    {
        var doc = BuildCommon(model, SearchResourceType.Concept, model.Identifiers.First());

        SetIfNotNull(doc, EsCatalogFields.Version, model.Version);
        SetSingleMultiLang(doc, EsCatalogFields.Name, model.Name);
        SetSingleMultiLang(doc, EsCatalogFields.Title, model.Name);
        doc[EsCatalogFields.Themes] = model.Themes.Select(x => x.Code).ToArray();
        doc[EsCatalogFields.ConceptType] = (int)model.ConceptType;
        SetIfNotNull(doc, EsCatalogFields.ValidFrom, model.ValidFrom?.ToString("o"));
        SetIfNotNull(doc, EsCatalogFields.ValidTo, model.ValidTo?.ToString("o"));
        AddPeople(doc, model.ResponsiblePerson, model.ResponsibleDeputy);

        return (GuidToString(model.Id), doc);
    }

    public static (string Id, Dictionary<string, object?> Document) FromMappingTable(MappingTableModel model)
    {
        var doc = BuildCommon(model, SearchResourceType.MappingTable, model.Identifiers.First());

        SetIfNotNull(doc, EsCatalogFields.Version, model.Version);
        SetSingleMultiLang(doc, EsCatalogFields.Name, model.Name);
        SetSingleMultiLang(doc, EsCatalogFields.Title, model.Name);
        doc[EsCatalogFields.Themes] = model.Themes.Select(x => x.Code).ToArray();
        SetIfNotNull(doc, EsCatalogFields.ValidFrom, model.ValidFrom?.ToString("o"));
        SetIfNotNull(doc, EsCatalogFields.ValidTo, model.ValidTo?.ToString("o"));
        AddPeople(doc, model.ResponsiblePerson, model.ResponsibleDeputy);

        return (GuidToString(model.Id), doc);
    }

    private static Dictionary<string, object?> BuildCommon(IPublishableEntityModel model, SearchResourceType type, string identifier)
    {
        var doc = new Dictionary<string, object?>
        {
            [EsCatalogFields.Id] = GuidToString(model.Id),
            [EsCatalogFields.Identifier] = identifier,
            [EsCatalogFields.PublicationLevel] = (int)model.PublicationLevel,
            [EsCatalogFields.Publisher] = model.Publisher.Id.ToString("N"),
            [EsCatalogFields.PublisherIdentifier] = model.Publisher.Identifier.ToLowerInvariant(),
            [EsCatalogFields.RegistrationStatus] = (int)model.RegistrationStatus,
            [EsCatalogFields.RegistrationStatusWeight] = RegistrationStatusToWeight(model.RegistrationStatus),
            [EsCatalogFields.Type] = type.ToString(),
            [EsCatalogFields.CreatedAt] = model.System.CreatedAt.ToString("o"),
        };

        if (model.PublicationLevelProposal.HasValue)
        {
            doc[EsCatalogFields.PublicationLevelProposal] = (int)model.PublicationLevelProposal.Value;
        }

        if (model.RegistrationStatusProposal.HasValue)
        {
            doc[EsCatalogFields.RegistrationStatusProposal] = (int)model.RegistrationStatusProposal.Value;
        }

        if (model.System.CreationType.HasValue)
        {
            doc[EsCatalogFields.CreationType] = (int)model.System.CreationType.Value;
        }

        if (model.System.ModifiedAt.HasValue)
        {
            doc[EsCatalogFields.ModifiedAt] = model.System.ModifiedAt.Value.ToString("o");
        }

        SetMultiValuedMultiLang(
            doc,
            EsCatalogFields.Keyword,
            model.Keywords.Where(x => x.Label is not null).Select(x => x.Label!));

        SetSingleMultiLang(doc, EsCatalogFields.Description, model.Description);

        return doc;
    }

    private static void AddPeople(Dictionary<string, object?> doc, IopPersonModel? person, IopPersonModel? deputy)
    {
        if (person is not null)
        {
            doc[EsCatalogFields.ResponsiblePersonEmail] = person.Email.ToLowerInvariant();
            doc[EsCatalogFields.ResponsiblePersonName] = $"{person.GivenName} {person.FamilyName}".Trim().ToLowerInvariant();
        }

        if (deputy is not null)
        {
            doc[EsCatalogFields.ResponsibleDeputyEmail] = deputy.Email.ToLowerInvariant();
            doc[EsCatalogFields.ResponsibleDeputyName] = $"{deputy.GivenName} {deputy.FamilyName}".Trim().ToLowerInvariant();
        }
    }

    private static void AddContactPoints(Dictionary<string, object?> doc, IEnumerable<VCardModel>? contactPoints)
    {
        if (contactPoints is null)
        {
            return;
        }

        var emails = new List<string>();
        foreach (var cp in contactPoints)
        {
            AccumulateMultiLang(doc, EsCatalogFields.ContactPointFn, cp.Fn);
            AccumulateMultiLang(doc, EsCatalogFields.ContactPointHasAddress, cp.HasAddress);
            AccumulateMultiLang(doc, EsCatalogFields.ContactPointNote, cp.Note);

            if (!string.IsNullOrWhiteSpace(cp.HasEmail))
            {
                emails.Add(cp.HasEmail.ToLowerInvariant());
            }
        }

        if (emails.Count > 0)
        {
            // Merge with any emails already present (e.g. public-service channels never hit this path,
            // but datasets/data-services can have several contact points).
            var existing = doc.TryGetValue(EsCatalogFields.ContactPointHasEmail, out var v) && v is IEnumerable<string> e
                ? e
                : [];
            doc[EsCatalogFields.ContactPointHasEmail] = existing.Concat(emails).Distinct().ToArray();
        }
    }

    // Single-valued multilingual field -> { "de": "..", "fr": ".." }.
    private static void SetSingleMultiLang(Dictionary<string, object?> doc, string field, MultiLanguageModel? model)
    {
        if (model is null)
        {
            return;
        }

        var byLang = model.ToDictionary();
        if (byLang.Count > 0)
        {
            doc[field] = byLang.ToDictionary(x => x.Key, x => (object?)x.Value);
        }
    }

    // Multi-valued multilingual field -> { "de": ["..",".."], "fr": [".."] }.
    private static void SetMultiValuedMultiLang(Dictionary<string, object?> doc, string field, IEnumerable<MultiLanguageModel> models)
    {
        var acc = new Dictionary<string, List<string>>();
        foreach (var model in models)
        {
            foreach (var (lang, text) in model.ToDictionary())
            {
                if (!acc.TryGetValue(lang, out var list))
                {
                    list = [];
                    acc[lang] = list;
                }

                list.Add(text);
            }
        }

        if (acc.Count > 0)
        {
            doc[field] = acc.ToDictionary(x => x.Key, x => (object?)x.Value.ToArray());
        }
    }

    // Accumulate a multilingual value into an existing multi-valued multilingual field.
    private static void AccumulateMultiLang(Dictionary<string, object?> doc, string field, MultiLanguageModel? model)
    {
        if (model is null)
        {
            return;
        }

        var current = doc.TryGetValue(field, out var v) && v is Dictionary<string, object?> existing
            ? existing
            : new Dictionary<string, object?>();

        foreach (var (lang, text) in model.ToDictionary())
        {
            var list = current.TryGetValue(lang, out var lv) && lv is List<string> l ? l : [];
            list.Add(text);
            current[lang] = list;
        }

        if (current.Count > 0)
        {
            doc[field] = current;
        }
    }

    private static void SetIfNotNull(Dictionary<string, object?> doc, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            doc[field] = value;
        }
    }

    // Same weights as the Lucene RegistrationStatusToWeight — the registration-status ranking rule.
    public static int RegistrationStatusToWeight(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Incomplete => 95,
        RegistrationStatus.Candidate => 98,
        RegistrationStatus.Recorded => 100,
        RegistrationStatus.Qualified => 102,
        RegistrationStatus.Standard => 105,
        RegistrationStatus.PreferredStandard => 110,
        RegistrationStatus.Superseded => 90,
        RegistrationStatus.Retired => 85,
        _ => 100
    };

    private static string GuidToString(Guid id) => id.ToString("D").ToLowerInvariant();
}
