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

    private static PagedResult<CatalogSearchHit> Read(string source, int total = 1)
    {
        using var document = JsonDocument.Parse(
            $"{{\"hits\":{{\"total\":{{\"value\":{total}}},\"hits\":[{{\"_source\":{{{source}}}}}]}}}}");

        return CatalogResponseReader.ReadSearch(document.RootElement, page: 1, pageSize: 20);
    }
}
