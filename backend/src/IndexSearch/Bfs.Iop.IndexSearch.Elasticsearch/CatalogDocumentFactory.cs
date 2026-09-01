using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

internal static class CatalogDocumentFactory
{
    public static (string Id, Dictionary<string, object?> Document) Build(CatalogIndexDocument entry)
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

        SetValues(doc, EsCatalogFields.Identifier, entry.Identifiers);
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

        SetDate(doc, EsCatalogFields.CreatedAt, entry.CreatedAt);
        SetDate(doc, EsCatalogFields.ModifiedAt, entry.ModifiedAt);
        SetDate(doc, EsCatalogFields.ValidFrom, entry.ValidFrom);
        SetDate(doc, EsCatalogFields.ValidTo, entry.ValidTo);

        SetValues(doc, EsCatalogFields.Themes, entry.Themes);
        SetValues(doc, EsCatalogFields.Formats, entry.Formats);
        SetValues(doc, EsCatalogFields.BusinessEvents, entry.BusinessEvents);
        SetValues(doc, EsCatalogFields.LifeEvents, entry.LifeEvents);

        if (entry.HasStructure.HasValue)
        {
            doc[EsCatalogFields.HasStructure] = entry.HasStructure.Value;
        }

        SetMultiLang(doc, EsCatalogFields.Title, entry.Title);
        SetMultiLang(doc, EsCatalogFields.Name, entry.Name);
        SetMultiLang(doc, EsCatalogFields.Description, entry.Description);
        SetMultiValuedMultiLang(doc, EsCatalogFields.Keyword, entry.Keywords);

        SetPerson(doc, entry.ResponsiblePerson, EsCatalogFields.ResponsiblePersonName, EsCatalogFields.ResponsiblePersonEmail);
        SetPerson(doc, entry.ResponsibleDeputy, EsCatalogFields.ResponsibleDeputyName, EsCatalogFields.ResponsibleDeputyEmail);

        AddContactPoints(doc, entry.ContactPoints);
        SetValues(doc, EsCatalogFields.ChannelEmail, [.. entry.ChannelEmails.Select(x => x.ToLowerInvariant())]);

        return (entry.Id.ToString(), doc);
    }

    private static int RegistrationStatusToWeight(IndexRegistrationStatus status) => status switch
    {
        IndexRegistrationStatus.Incomplete => 95,
        IndexRegistrationStatus.Candidate => 98,
        IndexRegistrationStatus.Recorded => 100,
        IndexRegistrationStatus.Qualified => 102,
        IndexRegistrationStatus.Standard => 105,
        IndexRegistrationStatus.PreferredStandard => 110,
        IndexRegistrationStatus.Superseded => 90,
        IndexRegistrationStatus.Retired => 85,
        _ => 100,
    };

    private static void SetIfPresent(Dictionary<string, object?> doc, string field, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            doc[field] = value;
        }
    }

    private static void SetDate(Dictionary<string, object?> doc, string field, DateTimeOffset? value)
    {
        if (value.HasValue)
        {
            doc[field] = value.Value.UtcDateTime;
        }
    }

    private static void SetValues(Dictionary<string, object?> doc, string field, IReadOnlyList<string> values)
    {
        var present = values.Where(x => !string.IsNullOrWhiteSpace(x)).ToArray();

        if (present.Length > 0)
        {
            doc[field] = present;
        }
    }

    private static void SetMultiLang(Dictionary<string, object?> doc, string field, LocalizedText? text)
    {
        if (text is null || text.IsEmpty)
        {
            return;
        }

        doc[field] = text.ToDictionary().ToDictionary(x => x.Key, x => (object?)x.Value);
    }

    private static void SetMultiValuedMultiLang(
        Dictionary<string, object?> doc,
        string field,
        IReadOnlyList<LocalizedText> texts)
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

    private static void AddContactPoints(Dictionary<string, object?> doc, IReadOnlyList<IndexContactPoint> contactPoints)
    {
        if (contactPoints.Count == 0)
        {
            return;
        }

        SetMultiValuedMultiLang(doc, EsCatalogFields.ContactPointFn,
            [.. contactPoints.Select(x => x.Fn).OfType<LocalizedText>()]);
        SetMultiValuedMultiLang(doc, EsCatalogFields.ContactPointHasAddress,
            [.. contactPoints.Select(x => x.HasAddress).OfType<LocalizedText>()]);
        SetMultiValuedMultiLang(doc, EsCatalogFields.ContactPointNote,
            [.. contactPoints.Select(x => x.Note).OfType<LocalizedText>()]);

        SetValues(doc, EsCatalogFields.ContactPointHasEmail,
            [.. contactPoints.Select(x => x.HasEmail).OfType<string>().Select(x => x.ToLowerInvariant())]);

        SetValues(doc, EsCatalogFields.ContactPointHasTelephone,
            [.. contactPoints.Select(x => x.HasTelephone).OfType<string>()]);
    }
}
