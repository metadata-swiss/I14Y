using AwesomeAssertions;
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
        Type = IndexResourceType.Dataset,
        PublisherIdentifier = "CH_BFS",
        RegistrationStatus = IndexRegistrationStatus.Recorded,
        PublicationLevel = IndexPublicationLevel.Public,
    };

    // These weights order the whole catalogue. They are carried over verbatim from the previous
    // engine, so a change here silently reranks every search result.
    [TestCase(IndexRegistrationStatus.Retired, 85)]
    [TestCase(IndexRegistrationStatus.Superseded, 90)]
    [TestCase(IndexRegistrationStatus.Incomplete, 95)]
    [TestCase(IndexRegistrationStatus.Candidate, 98)]
    [TestCase(IndexRegistrationStatus.Recorded, 100)]
    [TestCase(IndexRegistrationStatus.Qualified, 102)]
    [TestCase(IndexRegistrationStatus.Standard, 105)]
    [TestCase(IndexRegistrationStatus.PreferredStandard, 110)]
    public void Registration_status_weights_are_unchanged(IndexRegistrationStatus status, int expected)
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

    // Absent is not false: the flag lives in the object store, so a caller that does not know it
    // must not be able to clear it by omission.
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
        var (_, doc) = CatalogDocumentFactory.Build(Entry() with { Type = IndexResourceType.MappingTable });

        doc[EsCatalogFields.Type].Should().Be("MappingTable");
        doc[EsCatalogFields.RegistrationStatus].Should().Be("Recorded");
    }

    [Test]
    public void Multilingual_fields_keep_only_populated_languages()
    {
        var (_, doc) = CatalogDocumentFactory.Build(
            Entry() with { Title = new LocalizedText { De = "Titel", Fr = "  " } });

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
