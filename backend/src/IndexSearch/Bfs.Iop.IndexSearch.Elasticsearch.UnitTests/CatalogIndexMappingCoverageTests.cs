using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
public class CatalogIndexMappingCoverageTests
{
    private static HashSet<string> MappedFields()
    {
        using var doc = JsonDocument.Parse(CatalogIndexMapping.BuildCreateIndexJson());

        return [.. doc.RootElement
            .GetProperty("mappings")
            .GetProperty("properties")
            .EnumerateObject()
            .Select(x => x.Name)];
    }

    [Test]
    public void Every_declared_field_name_is_mapped()
    {
        var declared = typeof(EsCatalogFields)
            .GetFields()
            .Where(x => x.IsLiteral && x.FieldType == typeof(string))
            .Select(x => (string)x.GetRawConstantValue()!)
            .ToArray();

        MappedFields().Should().Contain(declared);
    }

    [Test]
    public void Every_field_a_fully_populated_document_emits_is_mapped()
    {
        var text = new MultiLanguageModel { De = "de", En = "en", Fr = "fr", It = "it", Rm = "rm" };

        var (_, document) = CatalogDocumentFactory.Build(new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = SearchResourceType.Dataset,
            Identifiers = ["ds-1", "ds-1-old"],
            PublisherId = Guid.NewGuid(),
            PublisherIdentifier = "CH_BFS",
            PublicationLevel = PublicationLevel.Public,
            PublicationLevelProposal = PublicationLevel.Internal,
            RegistrationStatus = RegistrationStatus.Recorded,
            RegistrationStatusProposal = RegistrationStatus.Qualified,
            CreatedAt = DateTimeOffset.UnixEpoch,
            ModifiedAt = DateTimeOffset.UnixEpoch,
            CreationType = CreationType.Manual,
            Title = text,
            Name = text,
            Description = text,
            Keywords = [text],
            Version = "1.0.0",
            DataOwner = "BFS",
            AccessRights = "PUBLIC",
            Themes = ["ENER"],
            Formats = ["CSV"],
            BusinessEvents = ["BE"],
            LifeEvents = ["LE"],
            HasStructure = true,
            ConceptType = ConceptType.CodeList,
            ValidFrom = DateTimeOffset.UnixEpoch,
            ValidTo = DateTimeOffset.UnixEpoch,
            ResponsiblePerson = new IndexPerson { GivenName = "Ada", FamilyName = "Lovelace", Email = "ada@example.ch" },
            ResponsibleDeputy = new IndexPerson { GivenName = "Alan", FamilyName = "Turing", Email = "alan@example.ch" },
            ContactPoints =
            [
                new IndexContactPoint
                {
                    Fn = text, HasAddress = text, Note = text, HasEmail = "k@example.ch", HasTelephone = "+41 58 000 00 00",
                },
            ],
            ChannelEmails = ["kanal@example.ch"],
        });

        MappedFields().Should().Contain(document.Keys);
    }
}
