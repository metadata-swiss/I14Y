using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The reader turns a server answer into the models the API returns, so it is the one place that reads
// JSON we did not write: an older document, a schema that has moved on, or an index Elasticsearch
// auto-created with a dynamic mapping. Every value it takes has to fail to one row rather than to the
// whole page, because a search that throws tells the caller nothing at all.
[TestFixture]
internal sealed class CatalogResponseReaderTests
{
    private static readonly Guid Id = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Test]
    public void A_hit_with_no_readable_id_is_dropped_and_counted()
    {
        var result = Read(Hit($"\"id\":\"{Id}\""), Hit("\"type\":\"Dataset\""), out var skipped);

        skipped.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Id.Should().Be(Id);
    }

    [TestCase("\"id\":42", TestName = "id as a number")]
    [TestCase("\"id\":\"not-a-guid\"", TestName = "id that is not a guid")]
    [TestCase("\"type\":\"Dataset\"", TestName = "no id at all")]
    public void An_unusable_id_costs_its_own_row_and_not_the_page(string body)
    {
        var read = () => Read(Hit(body), out _);

        // CatalogSearchHit.Id is a required Guid, so there is nothing to render for such a document.
        // Before this it threw out of the Select and took every other hit on the page with it.
        read.Should().NotThrow();
    }

    [Test]
    public void The_total_still_counts_a_document_that_could_not_be_read()
    {
        // Elasticsearch counted it as a match, and it was one. Reporting Results.Count instead would
        // claim the index holds fewer documents than it does.
        var result = Read(Hit("\"type\":\"Dataset\""), out var skipped);

        result.TotalCount.Should().Be(1);
        result.Results.Should().BeEmpty();
        skipped.Should().Be(1);
    }

    [TestCase("[]", TestName = "an empty array")]
    [TestCase("[5]", TestName = "an array holding no string")]
    public void A_language_holding_no_usable_text_reads_as_absent(string title)
    {
        // FirstOrDefault over a struct hands back default(JsonElement), whose ValueKind is Undefined
        // and whose GetString() throws rather than returning null.
        var result = Read(Hit($"\"id\":\"{Id}\",\"title\":{{\"de\":{title}}}"), out _);

        result.Results.Single().Title.Should().BeNull();
    }

    [Test]
    public void A_language_holding_several_values_reads_the_first_string()
    {
        var result = Read(Hit($"\"id\":\"{Id}\",\"title\":{{\"de\":[5,\"Zweite\"]}}"), out _);

        result.Results.Single().Title!.De.Should().Be("Zweite");
    }

    [TestCase("\"true\"", TestName = "hasStructure as a string")]
    [TestCase("1", TestName = "hasStructure as a number")]
    public void A_structure_flag_of_the_wrong_type_reads_as_unknown(string value)
    {
        // What a dynamically mapped, auto-created index produces. Every sibling field guards its
        // ValueKind; this one called GetBoolean() bare and threw.
        var result = Read(Hit($"\"id\":\"{Id}\",\"hasStructure\":{value}"), out _);

        result.Results.Single().HasStructure.Should().BeNull();
    }

    [Test]
    public void A_structure_flag_that_is_a_boolean_is_read()
    {
        var result = Read(Hit($"\"id\":\"{Id}\",\"hasStructure\":true"), out _);

        result.Results.Single().HasStructure.Should().BeTrue();
    }

    [Test]
    public void A_multi_valued_field_holding_one_bare_string_reads_as_a_list()
    {
        var result = Read(Hit($"\"id\":\"{Id}\",\"identifier\":\"BFS-1\""), out _);

        result.Results.Single().Identifiers.Should().Equal("BFS-1");
    }

    private static PagedResult<CatalogSearchHit> Read(string hit, out int skipped) => Read(hit, null, out skipped);

    private static PagedResult<CatalogSearchHit> Read(string first, string? second, out int skipped)
    {
        var hits = second is null ? first : $"{first},{second}";
        var total = second is null ? 1 : 2;

        using var document = JsonDocument.Parse(
            $"{{\"hits\":{{\"total\":{{\"value\":{total}}},\"hits\":[{hits}]}}}}");

        return CatalogResponseReader.ReadSearch(document.RootElement, page: 1, pageSize: 20, out skipped);
    }

    private static string Hit(string source) => $"{{\"_source\":{{{source}}}}}";
}
