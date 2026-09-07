using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(CatalogDocumentFactory))]
public class CatalogDocumentFactoryTests
{
    private static CatalogIndexDocument Entry(Action<CatalogIndexDocument>? _ = null) => new()
    {
        Id = Guid.NewGuid(),
        Type = SearchResourceType.Dataset,
        PublisherIdentifier = "CH_BFS",
        RegistrationStatus = RegistrationStatus.Recorded,
        PublicationLevel = PublicationLevel.Public,
    };

    [TestCase(RegistrationStatus.Retired, 85)]
    [TestCase(RegistrationStatus.Superseded, 90)]
    [TestCase(RegistrationStatus.Incomplete, 95)]
    [TestCase(RegistrationStatus.Candidate, 98)]
    [TestCase(RegistrationStatus.Recorded, 100)]
    [TestCase(RegistrationStatus.Qualified, 102)]
    [TestCase(RegistrationStatus.Standard, 105)]
    [TestCase(RegistrationStatus.PreferredStandard, 110)]
    public void Registration_status_weights_are_unchanged(RegistrationStatus status, int expected)
    {
        var (_, doc) = CatalogDocumentFactory.Build(Entry() with { RegistrationStatus = status });

        doc[EsCatalogFields.RegistrationStatusWeight].Should().Be(expected);
    }

    [Test]
    public void Publisher_identifier_is_written_in_both_casings()
    {
        var (_, doc) = CatalogDocumentFactory.Build(Entry());

        doc[EsCatalogFields.PublisherIdentifier].Should().Be("ch_bfs");
        doc[EsCatalogFields.PublisherIdentifierLabel].Should().Be("CH_BFS");
    }

    [Test]
    public void HasStructure_is_omitted_when_null_and_written_when_set()
    {
        CatalogDocumentFactory.Build(Entry()).Document
            .Should().NotContainKey(EsCatalogFields.HasStructure);

        CatalogDocumentFactory.Build(Entry() with { HasStructure = false }).Document[EsCatalogFields.HasStructure]
            .Should().Be(false);
    }

    [Test]
    public void Enums_are_written_as_names()
    {
        var (_, doc) = CatalogDocumentFactory.Build(Entry() with { Type = SearchResourceType.MappingTable });

        doc[EsCatalogFields.Type].Should().Be("MappingTable");
        doc[EsCatalogFields.RegistrationStatus].Should().Be("Recorded");
    }

    [Test]
    public void Multilingual_fields_keep_only_populated_languages()
    {
        var (_, doc) = CatalogDocumentFactory.Build(
            Entry() with { Title = new MultiLanguageModel { De = "Titel", Fr = "  " } });

        doc[EsCatalogFields.Title].Should().BeEquivalentTo(new Dictionary<string, object?> { ["de"] = "Titel" });
    }

    [Test]
    public void Empty_collections_are_not_written()
    {
        var (_, doc) = CatalogDocumentFactory.Build(Entry());

        doc.Should().NotContainKey(EsCatalogFields.Themes)
            .And.NotContainKey(EsCatalogFields.Formats)
            .And.NotContainKey(EsCatalogFields.ChannelEmail);
    }
}
