using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogDocumentFactory
{
    public static IndexRequest Build(CatalogIndexDocument entry)
    {
        ArgumentNullException.ThrowIfNull(entry);

        var doc = new Dictionary<string, object?>
        {
            [EsCatalogFields.Id] = entry.Id.ToString(),
            [EsCatalogFields.Type] = entry.Type.ToString(),
            [EsCatalogFields.PublicationLevel] = entry.PublicationLevel.ToString(),
            [EsCatalogFields.RegistrationStatus] = entry.RegistrationStatus.ToString(),
            [EsCatalogFields.RegistrationStatusWeight] = RegistrationStatusToWeight(entry.RegistrationStatus),
            [EsCatalogFields.CreationType] = entry.CreationType.ToString(),
            [EsCatalogFields.Publisher] = entry.PublisherId.ToString(),
        };

        SetIfPresent(doc, EsCatalogFields.Identifier, entry.Identifiers);
        SetIfPresent(doc, EsCatalogFields.Version, entry.Version);
        SetIfPresent(doc, EsCatalogFields.DataOwner, entry.DataOwner);
        SetIfPresent(doc, EsCatalogFields.AccessRights, entry.AccessRights);

        if (!string.IsNullOrWhiteSpace(entry.PublisherIdentifier))
        {
            doc[EsCatalogFields.PublisherIdentifier] = entry.PublisherIdentifier.ToLowerInvariant();
            doc[EsCatalogFields.PublisherIdentifierLabel] = entry.PublisherIdentifier;
        }

        SetIfPresent(doc, EsCatalogFields.PublicationLevelProposal, entry.PublicationLevelProposal?.ToString());
        SetIfPresent(doc, EsCatalogFields.RegistrationStatusProposal, entry.RegistrationStatusProposal?.ToString());
        SetIfPresent(doc, EsCatalogFields.ConceptType, entry.ConceptType?.ToString());

        SetIfPresent(doc, EsCatalogFields.CreatedAt, entry.CreatedAt);
        SetIfPresent(doc, EsCatalogFields.ModifiedAt, entry.ModifiedAt);
        SetIfPresent(doc, EsCatalogFields.ValidFrom, entry.ValidFrom);
        SetIfPresent(doc, EsCatalogFields.ValidTo, entry.ValidTo);

        SetIfPresent(doc, EsCatalogFields.Themes, entry.Themes);
        SetIfPresent(doc, EsCatalogFields.Formats, entry.Formats);
        SetIfPresent(doc, EsCatalogFields.BusinessEvents, entry.BusinessEvents);
        SetIfPresent(doc, EsCatalogFields.LifeEvents, entry.LifeEvents);

        if (entry.HasStructure.HasValue)
        {
            doc[EsCatalogFields.HasStructure] = entry.HasStructure.Value;
        }

        SetIfPresent(doc, EsCatalogFields.Title, entry.Title);
        SetIfPresent(doc, EsCatalogFields.Name, entry.Name);
        SetIfPresent(doc, EsCatalogFields.Description, entry.Description);
        SetIfPresent(doc, EsCatalogFields.Keyword, entry.Keywords);

        SetPerson(doc, entry.ResponsiblePerson, EsCatalogFields.ResponsiblePersonName, EsCatalogFields.ResponsiblePersonEmail);
        SetPerson(doc, entry.ResponsibleDeputy, EsCatalogFields.ResponsibleDeputyName, EsCatalogFields.ResponsibleDeputyEmail);

        SetContactPoints(doc, entry.ContactPoints);
        SetIfPresent(doc, EsCatalogFields.ChannelEmail,
            [.. entry.ChannelEmails.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.ToLowerInvariant())]);

        return new IndexRequest(entry.Id.ToString(), doc);
    }

    private static int RegistrationStatusToWeight(RegistrationStatus status) => status switch
    {
        RegistrationStatus.Incomplete => 95,
        RegistrationStatus.Candidate => 98,
        RegistrationStatus.Recorded => 100,
        RegistrationStatus.Qualified => 102,
        RegistrationStatus.Standard => 105,
        RegistrationStatus.PreferredStandard => 110,
        RegistrationStatus.Superseded => 90,
        RegistrationStatus.Retired => 85,
        _ => 100,
    };

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            doc[field] = value;
        }
    }

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            doc[field] = value.Value.UtcDateTime;
        }
    }

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, IReadOnlyList<string> values)
    {
        var present = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        if (present.Length > 0)
        {
            doc[field] = present;
        }
    }

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, MultiLanguageModel? text)
    {
        if (text is null || text.IsContentNullOrWhiteSpace())
        {
            return;
        }

        doc[field] = text.ToDictionary().ToDictionary(x => x.Key, x => (object?)x.Value);
    }

    private static void SetIfPresent(
        Dictionary<string, object?> doc,
        string field,
        IReadOnlyList<MultiLanguageModel> texts)
    {
        var byLanguage = new Dictionary<string, List<string>>(StringComparer.Ordinal);

        foreach (var text in texts)
        {
            foreach (var (language, value) in text.ToDictionary())
            {
                if (!byLanguage.TryGetValue(language, out var values))
                {
                    values = [];
                    byLanguage[language] = values;
                }

                values.Add(value);
            }
        }

        if (byLanguage.Count > 0)
        {
            doc[field] = byLanguage.ToDictionary(x => x.Key, x => (object?)x.Value.ToArray());
        }
    }

    private static void SetPerson(
        Dictionary<string, object?> doc,
        IndexPerson? person,
        string nameField,
        string emailField)
    {
        if (person is null)
        {
            return;
        }

        var name = string.Join(' ', new[] { person.GivenName, person.FamilyName }
            .Where(x => !string.IsNullOrWhiteSpace(x)));

        SetIfPresent(doc, nameField, name);
        SetIfPresent(doc, emailField, person.Email?.ToLowerInvariant());
    }

    private static void SetContactPoints(Dictionary<string, object?> doc, IReadOnlyList<IndexContactPoint> contactPoints)
    {
        if (contactPoints.Count == 0)
        {
            return;
        }

        SetIfPresent(doc, EsCatalogFields.ContactPointFn,
            [.. contactPoints.Select(x => x.Fn).OfType<MultiLanguageModel>()]);
        SetIfPresent(doc, EsCatalogFields.ContactPointHasAddress,
            [.. contactPoints.Select(x => x.HasAddress).OfType<MultiLanguageModel>()]);
        SetIfPresent(doc, EsCatalogFields.ContactPointNote,
            [.. contactPoints.Select(x => x.Note).OfType<MultiLanguageModel>()]);

        SetIfPresent(doc, EsCatalogFields.ContactPointHasEmail,
            [.. contactPoints.Select(x => x.HasEmail).OfType<string>().Select(x => x.ToLowerInvariant())]);

        SetIfPresent(doc, EsCatalogFields.ContactPointHasTelephone,
            [.. contactPoints.Select(x => x.HasTelephone).OfType<string>()]);
    }
}
