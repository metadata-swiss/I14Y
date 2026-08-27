using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Indexing;

namespace Bfs.Iop.Search.Elasticsearch;

/// <summary>
/// Builds the JSON document (as a dictionary) indexed for each catalog resource.
/// <para>
/// Takes a <see cref="CatalogIndexEntry"/> — a flat projection carrying exactly the fields below and
/// nothing else. One input shape for both producers: the full rebuild projects it from the database,
/// a single write projects it from the domain model. There is no second <c>From*</c> family to keep
/// in step.
/// </para>
/// <para>
/// Boosts are NOT applied here. They are applied at query time in
/// <see cref="CatalogQueryBuilder"/>, which is the Elasticsearch idiom: a boost baked into the
/// document can only be changed by reindexing every resource, whereas a query-time boost takes
/// effect on the next search.
/// </para>
/// </summary>
internal static class CatalogDocumentFactory
{
    public static (string Id, Dictionary<string, object?> Document) Build(CatalogIndexEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var doc = new Dictionary<string, object?>
        {
            [EsCatalogFields.Id] = GuidToString(entry.Id),
            [EsCatalogFields.Identifier] = entry.Identifier,
            [EsCatalogFields.PublicationLevel] = (int)entry.PublicationLevel,
            [EsCatalogFields.Publisher] = entry.PublisherId.ToString("N"),
            // Written twice, deliberately: lowercased for term matching (authorization, filters) and
            // case-preserved for the facet, whose bucket keys have to survive a case-sensitive agent
            // lookup. See EsCatalogFields.PublisherIdentifierLabel.
            [EsCatalogFields.PublisherIdentifier] = entry.PublisherIdentifier.ToLowerInvariant(),
            [EsCatalogFields.PublisherIdentifierLabel] = entry.PublisherIdentifier,
            [EsCatalogFields.RegistrationStatus] = (int)entry.RegistrationStatus,
            [EsCatalogFields.RegistrationStatusWeight] = RegistrationStatusToWeight(entry.RegistrationStatus),
            [EsCatalogFields.Type] = entry.Type.ToString(),
            [EsCatalogFields.CreatedAt] = entry.CreatedAt.ToString("o"),
            [EsCatalogFields.Themes] = entry.Themes.ToArray(),
        };

        if (entry.PublicationLevelProposal.HasValue)
        {
            doc[EsCatalogFields.PublicationLevelProposal] = (int)entry.PublicationLevelProposal.Value;
        }

        if (entry.RegistrationStatusProposal.HasValue)
        {
            doc[EsCatalogFields.RegistrationStatusProposal] = (int)entry.RegistrationStatusProposal.Value;
        }

        if (entry.CreationType.HasValue)
        {
            doc[EsCatalogFields.CreationType] = (int)entry.CreationType.Value;
        }

        if (entry.ModifiedAt.HasValue)
        {
            doc[EsCatalogFields.ModifiedAt] = entry.ModifiedAt.Value.ToString("o");
        }

        SetMultiValuedMultiLang(doc, EsCatalogFields.Keyword, entry.Keywords);
        SetSingleMultiLang(doc, EsCatalogFields.Description, entry.Description);
        SetSingleMultiLang(doc, EsCatalogFields.Title, entry.Title);
        SetSingleMultiLang(doc, EsCatalogFields.Name, entry.Name);

        SetIfNotNull(doc, EsCatalogFields.Version, entry.Version);

        // Present-but-empty is meaningful for the array fields: an aggregation over a missing field
        // yields no buckets, so writing the empty array keeps a resource countable under "no themes".
        // Only write the ones the resource kind actually has.
        if (entry.AccessRights is not null || entry.Type is SearchResourceType.Dataset or SearchResourceType.DataService)
        {
            doc[EsCatalogFields.AccessRights] = entry.AccessRights;
        }

        if (entry.Type is SearchResourceType.Dataset)
        {
            doc[EsCatalogFields.Formats] = entry.Formats.ToArray();
        }

        if (entry.Type is SearchResourceType.PublicService)
        {
            doc[EsCatalogFields.BusinessEvents] = entry.BusinessEvents.ToArray();
            doc[EsCatalogFields.LifeEvents] = entry.LifeEvents.ToArray();
        }

        if (!string.IsNullOrWhiteSpace(entry.DataOwner))
        {
            doc[EsCatalogFields.DataOwner] = entry.DataOwner.Trim().ToLowerInvariant();
        }

        if (entry.ConceptType.HasValue)
        {
            doc[EsCatalogFields.ConceptType] = (int)entry.ConceptType.Value;
        }

        SetIfNotNull(doc, EsCatalogFields.ValidFrom, entry.ValidFrom?.ToString("o"));
        SetIfNotNull(doc, EsCatalogFields.ValidTo, entry.ValidTo?.ToString("o"));

        if (entry.HasStructure.HasValue)
        {
            doc[EsCatalogFields.HasStructure] = entry.HasStructure.Value;
        }

        AddPeople(doc, entry.ResponsiblePerson, entry.ResponsibleDeputy);
        AddContactPoints(doc, entry.ContactPoints, entry.ChannelEmails);

        return (GuidToString(entry.Id), doc);
    }

    private static void AddPeople(Dictionary<string, object?> doc, IndexPerson? person, IndexPerson? deputy)
    {
        if (person is not null)
        {
            doc[EsCatalogFields.ResponsiblePersonEmail] = person.Email.ToLowerInvariant();
            doc[EsCatalogFields.ResponsiblePersonName] = FullName(person);
        }

        if (deputy is not null)
        {
            doc[EsCatalogFields.ResponsibleDeputyEmail] = deputy.Email.ToLowerInvariant();
            doc[EsCatalogFields.ResponsibleDeputyName] = FullName(deputy);
        }
    }

    private static string FullName(IndexPerson person) =>
        $"{person.GivenName} {person.FamilyName}".Trim().ToLowerInvariant();

    private static void AddContactPoints(
        Dictionary<string, object?> doc,
        IReadOnlyList<IndexContactPoint> contactPoints,
        IReadOnlyList<string> channelEmails)
    {
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

        // Public-service channel addresses land in the same field: someone searching for an address
        // does not know whether it was recorded as a contact point or a channel.
        emails.AddRange(channelEmails
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant()));

        if (emails.Count > 0)
        {
            doc[EsCatalogFields.ContactPointHasEmail] = emails.Distinct().ToArray();
        }
    }

    // The registration-status ranking rule. These weights order the catalogue: raising one floats
    // every resource in that status up the results. Changing a number here is a visible ranking
    // change, not a tuning detail.
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

    private static void SetIfNotNull(Dictionary<string, object?> doc, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            doc[field] = value;
        }
    }

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
}
