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
        TenantIdentifier = "CH_BFS",
        PublisherIdentifier = "CH_ESTV",
        PublisherName = new MultiLanguageModel { De = "Steuerverwaltung" },
        RegistrationStatus = RegistrationStatus.Recorded,
        PublicationLevel = PublicationLevel.Public,
    };

    [Test]
    public void A_reuse_count_of_zero_is_written()
    {
        // Zero means "indexed, nobody reuses it" and has to reach the index as a value, or it sorts
        // alongside the types that have no reuse notion at all.
        var document = CatalogDocumentFactory.Build(Entry() with { ReuseCount = 0 }, DefaultScoring.Catalog).Body;

        document[EsCatalogFields.ReuseCount].Should().Be(0);
    }

    [Test]
    public void An_absent_reuse_count_is_left_out_rather_than_written_as_zero()
    {
        var document = CatalogDocumentFactory.Build(Entry() with { ReuseCount = null }, DefaultScoring.Catalog).Body;

        document.Should().NotContainKey(EsCatalogFields.ReuseCount);
    }

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
        var doc = CatalogDocumentFactory.Build(Entry() with { RegistrationStatus = status }, DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.RegistrationStatusWeight].Should().Be(expected);
    }

    [Test]
    public void Tenant_identifier_is_written_in_both_casings()
    {
        var doc = CatalogDocumentFactory.Build(Entry(), DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.TenantIdentifier].Should().Be("ch_bfs");
        doc[EsCatalogFields.TenantIdentifierLabel].Should().Be("CH_BFS");
    }

    [Test]
    public void Publisher_identifier_is_written_in_both_casings_with_its_name()
    {
        var doc = CatalogDocumentFactory.Build(Entry(), DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.PublisherIdentifier].Should().Be("ch_estv");
        doc[EsCatalogFields.PublisherIdentifierLabel].Should().Be("CH_ESTV");
        doc[EsCatalogFields.PublisherName].Should().BeEquivalentTo(
            new Dictionary<string, object?> { [MultiLanguageModel.GermanKey] = "Steuerverwaltung" });
    }

    [Test]
    public void A_publisher_with_only_a_name_keeps_the_name_but_no_identifier()
    {
        var doc = CatalogDocumentFactory.Build(Entry() with { PublisherIdentifier = " " }, DefaultScoring.Catalog).Body;

        doc.Should().NotContainKeys(EsCatalogFields.PublisherIdentifier, EsCatalogFields.PublisherIdentifierLabel);
        doc[EsCatalogFields.PublisherName].Should().BeEquivalentTo(
            new Dictionary<string, object?> { [MultiLanguageModel.GermanKey] = "Steuerverwaltung" });
    }

    [Test]
    public void No_publisher_writes_no_publisher_fields()
    {
        var doc = CatalogDocumentFactory.Build(
            Entry() with { PublisherIdentifier = null, PublisherName = null },
            DefaultScoring.Catalog).Body;

        doc.Should().NotContainKeys(
            EsCatalogFields.PublisherIdentifier, EsCatalogFields.PublisherIdentifierLabel, EsCatalogFields.PublisherName);
    }

    [Test]
    public void HasStructure_is_omitted_when_null_and_written_when_set()
    {
        CatalogDocumentFactory.Build(Entry(), DefaultScoring.Catalog).Body
            .Should().NotContainKey(EsCatalogFields.HasStructure);

        CatalogDocumentFactory.Build(Entry() with { HasStructure = false }, DefaultScoring.Catalog).Body[EsCatalogFields.HasStructure]
            .Should().Be(false);
    }

    [Test]
    public void Enums_are_written_as_names()
    {
        var doc = CatalogDocumentFactory.Build(Entry() with { Type = SearchResourceType.MappingTable }, DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.Type].Should().Be("MappingTable");
        doc[EsCatalogFields.RegistrationStatus].Should().Be("Recorded");
    }

    [Test]
    public void Multilingual_fields_keep_only_populated_languages()
    {
        var doc = CatalogDocumentFactory.Build(
            Entry() with { Title = new MultiLanguageModel { De = "Titel", Fr = "  " } }, DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.Title].Should().BeEquivalentTo(new Dictionary<string, object?> { ["de"] = "Titel" });
    }

    [Test]
    public void Empty_collections_are_not_written()
    {
        var doc = CatalogDocumentFactory.Build(Entry(), DefaultScoring.Catalog).Body;

        doc.Should().NotContainKey(EsCatalogFields.Themes)
            .And.NotContainKey(EsCatalogFields.Formats)
            .And.NotContainKey(EsCatalogFields.ChannelEmail);
    }

    [Test]
    public void Catalogue_ids_are_written_in_the_form_the_filter_sends()
    {
        var catalog = Guid.NewGuid();

        var doc = CatalogDocumentFactory.Build(Entry() with { DcatCatalogs = [catalog] }, DefaultScoring.Catalog).Body;

        doc[EsCatalogFields.DcatCatalogs].Should().BeEquivalentTo(new[] { catalog.ToString() });
    }

    [Test]
    public void A_resource_in_no_catalogue_has_no_catalogue_field()
    {
        var doc = CatalogDocumentFactory.Build(Entry(), DefaultScoring.Catalog).Body;

        doc.Should().NotContainKey(EsCatalogFields.DcatCatalogs);
    }
}
