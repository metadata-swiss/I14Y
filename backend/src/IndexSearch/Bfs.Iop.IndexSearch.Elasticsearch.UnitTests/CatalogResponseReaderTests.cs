using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The reader turns a server answer into the models the API returns. A document whose id cannot be read
// means the index is broken and wants rebuilding, so that fails loudly rather than being papered over;
// what is covered here is the mapping of the fields themselves, which nothing else in CI exercises.
[TestFixture]
internal sealed class CatalogResponseReaderTests
{
    private static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public void A_hit_is_mapped_from_the_document_source()
    {
        var hit = Read($"\"id\":\"{Id}\",\"type\":\"Dataset\",\"version\":\"1.2.0\"").Results.Single();

        hit.Id.Should().Be(Id);
        hit.Type.Should().Be(SearchResourceType.Dataset);
        hit.Version.Should().Be("1.2.0");
    }

    [Test]
    public void The_total_comes_from_the_server_and_not_from_the_rows_returned()
    {
        // A page is 20 rows out of thousands, so the total can never be inferred from what came back.
        Read($"\"id\":\"{Id}\"", total: 2_935).TotalCount.Should().Be(2_935);
    }

    [TestCase("[5,\"Zweite\"]", TestName = "the first string of several values")]
    [TestCase("\"Erste\"", TestName = "a bare string")]
    public void A_language_reads_the_text_whatever_shape_it_is_stored_in(string title)
    {
        // Keywords are stored per language as arrays, plain text fields as strings; a hit renders the
        // same either way.
        var hit = Read($"\"id\":\"{Id}\",\"title\":{{\"de\":{title}}}").Results.Single();

        hit.Title!.De.Should().NotBeNullOrWhiteSpace();
    }

    [Test]
    public void A_multi_valued_field_holding_one_bare_string_reads_as_a_list()
    {
        // Elasticsearch accepts a scalar where an array is mapped and returns it the way it was sent.
        Read($"\"id\":\"{Id}\",\"identifier\":\"BFS-1\"").Results.Single()
            .Identifiers.Should().Equal("BFS-1");
    }

    [Test]
    public void A_field_the_document_does_not_carry_reads_as_absent()
    {
        var hit = Read($"\"id\":\"{Id}\"").Results.Single();

        hit.Title.Should().BeNull();
        hit.Themes.Should().BeEmpty();
        hit.HasStructure.Should().BeNull();
    }

    [Test]
    public void A_structure_flag_that_is_set_is_read()
    {
        Read($"\"id\":\"{Id}\",\"hasStructure\":true").Results.Single()
            .HasStructure.Should().BeTrue();
    }

    [Test]
    public void A_hit_carries_its_tenant_and_publisher()
    {
        var tenant = Guid.NewGuid();

        var hit = Read(
            $"\"id\":\"{Id}\",\"tenant\":\"{tenant}\",\"publisherIdentifier\":\"ch_estv\","
            + "\"publisherIdentifierLabel\":\"CH_ESTV\",\"publisherName\":{\"de\":\"Steuerverwaltung\"}").Results.Single();

        hit.TenantId.Should().Be(tenant);
        hit.PublisherIdentifier.Should().Be("CH_ESTV", because: "the result shows the identifier as it was entered");
        hit.PublisherName!.De.Should().Be("Steuerverwaltung");
    }

    [Test]
    public void A_hit_indexed_before_the_tenant_split_takes_its_tenant_from_the_former_publisher_field()
    {
        var tenant = Guid.NewGuid();

        Read($"\"id\":\"{Id}\",\"publisher\":\"{tenant}\"").Results.Single()
            .TenantId.Should().Be(tenant);
    }

    [Test]
    public void The_tenant_field_wins_over_the_former_publisher_field()
    {
        var tenant = Guid.NewGuid();

        Read($"\"id\":\"{Id}\",\"tenant\":\"{tenant}\",\"publisher\":\"{Guid.NewGuid()}\"").Results.Single()
            .TenantId.Should().Be(tenant);
    }

    [Test]
    public void Publisher_buckets_carry_their_name_and_tenant_buckets_their_count()
    {
        using var document = JsonDocument.Parse(
            """
            {"aggregations":{
              "TenantIdentifier":{"values":{"buckets":[{"key":"CH_BFS","doc_count":4}]}},
              "PublisherIdentifier":{"values":{"buckets":[
                {"key":"CH_ESTV","doc_count":3,"publisherDetails":{"hits":{"hits":[{"_source":{"publisherIdentifierLabel":"CH_ESTV","publisherName":{"de":"Steuerverwaltung"}}}]}}},
                {"key":"EXTERNAL","doc_count":1,"publisherDetails":{"hits":{"hits":[{"_source":{"publisherIdentifierLabel":"EXTERNAL"}}]}}},
                {"key":"Kanton Bern","doc_count":2,"publisherDetails":{"hits":{"hits":[{"_source":{"publisherName":{"de":"Kanton Bern"}}}]}}}
              ]}}
            }}
            """);

        var facets = CatalogResponseReader.ReadFacets(document.RootElement);

        facets.Tenants.Should().Equal(new Dictionary<string, int> { ["CH_BFS"] = 4 });
        facets.Publishers["CH_ESTV"].Count.Should().Be(3);
        facets.Publishers["CH_ESTV"].Identifier.Should().Be("CH_ESTV");
        facets.Publishers["CH_ESTV"].Name!.De.Should().Be("Steuerverwaltung");
        facets.Publishers["EXTERNAL"].Count.Should().Be(1);
        facets.Publishers["EXTERNAL"].Identifier.Should().Be("EXTERNAL");
        facets.Publishers["EXTERNAL"].Name.Should().BeNull();
        facets.Publishers["Kanton Bern"].Count.Should().Be(2);
        facets.Publishers["Kanton Bern"].Identifier.Should().BeNull("a publisher with only a name is keyed by its name");
        facets.Publishers["Kanton Bern"].Name!.De.Should().Be("Kanton Bern");
    }

    private static PagedResult<CatalogSearchHit> Read(string source, int total = 1)
    {
        using var document = JsonDocument.Parse(
            $"{{\"hits\":{{\"total\":{{\"value\":{total}}},\"hits\":[{{\"_source\":{{{source}}}}}]}}}}");

        return CatalogResponseReader.ReadSearch(document.RootElement, page: 1, pageSize: 20);
    }
}
