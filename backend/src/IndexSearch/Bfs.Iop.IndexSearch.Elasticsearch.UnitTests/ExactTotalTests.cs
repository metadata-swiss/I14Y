using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// Elasticsearch stops counting matches at 10 000 and answers hits.total.relation "gte" rather than
// "eq" unless the search asks for the real figure. Both readers publish hits.total.value as an exact
// TotalCount, so the default silently understates it: measured against the live index, a concept of
// 24 726 entries reported 10 000.
[TestFixture]
internal sealed class ExactTotalTests
{
    private static readonly string[] German = ["de"];

    [Test]
    public void A_code_list_search_asks_for_the_real_total()
    {
        Body(CodeListQueryBuilder.BuildSearchBody(
            Guid.NewGuid(), "statis", "de", filter: null, from: 0, size: 20))
            .GetProperty("track_total_hits").GetBoolean().Should().BeTrue();
    }

    [Test]
    public void A_catalog_search_asks_for_the_real_total()
    {
        // Smaller than the cap today, but the reader treats the figure as exact regardless, so the
        // catalog is only correct by accident of its current size.
        Body(CatalogQueryBuilder.BuildSearchBody(
            "statis", German, filter: null, SearchCaller.Anonymous, from: 0, size: 20))
            .GetProperty("track_total_hits").GetBoolean().Should().BeTrue();
    }

    [Test]
    public void The_facet_count_does_not_need_it()
    {
        // Aggregations are never capped, and the catalog total comes from the Total aggregation's
        // doc_count precisely so it reflects the facet selections. Asking here would only cost.
        Body(CatalogQueryBuilder.BuildCountBody("statis", German, filter: null, SearchCaller.Anonymous))
            .TryGetProperty("track_total_hits", out _).Should().BeFalse();
    }

    private static JsonElement Body(Dictionary<string, object?> body)
    {
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(body));

        return document.RootElement.Clone();
    }
}
