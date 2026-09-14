using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The code list is the larger of the two indices by far, so one unreadable entry among 490k must not
// be able to fail the page it happens to land on. Both ids are required on the hit, which is why an
// entry missing either is dropped rather than rendered half-formed.
[TestFixture]
internal sealed class CodeListResponseReaderTests
{
    private const string Id = "11111111-1111-1111-1111-111111111111";
    private const string ConceptId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    private static readonly string Complete = $"\"id\":\"{Id}\",\"conceptId\":\"{ConceptId}\",\"code\":\"01.02\"";

    [Test]
    public void An_entry_with_both_ids_is_read()
    {
        var result = Read(Hit(Complete), out var skipped);

        skipped.Should().Be(0);
        result.Results.Single().Code.Should().Be("01.02");
    }

    [TestCase("\"conceptId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\"", TestName = "no id")]
    [TestCase("\"id\":\"11111111-1111-1111-1111-111111111111\"", TestName = "no concept id")]
    [TestCase("\"id\":42,\"conceptId\":\"aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa\"", TestName = "id as a number")]
    public void An_entry_missing_either_id_is_dropped_rather_than_thrown_on(string source)
    {
        var result = Read(Hit(source), out var skipped);

        skipped.Should().Be(1);
        result.Results.Should().BeEmpty();
    }

    [Test]
    public void One_unreadable_entry_does_not_cost_the_others()
    {
        var result = Read(Hit(Complete), Hit("\"code\":\"99.99\""), out var skipped);

        skipped.Should().Be(1);
        result.Results.Should().ContainSingle().Which.Code.Should().Be("01.02");

        // Elasticsearch matched both, so the total says two even though one could not be rendered.
        result.TotalCount.Should().Be(2);
    }

    [Test]
    public void Annotations_that_are_not_an_array_read_as_none()
    {
        var result = Read(Hit($"{Complete},\"annotations\":\"note\""), out _);

        result.Results.Single().Annotations.Should().BeEmpty();
    }

    [Test]
    public void A_name_whose_languages_are_all_blank_reads_as_absent()
    {
        // The model treats whitespace as no content, so returning an empty MultiLanguageModel here
        // would make the front end render an empty label instead of falling back.
        var result = Read(Hit($"{Complete},\"name\":{{\"de\":\"  \"}}"), out _);

        result.Results.Single().Name.Should().BeNull();
    }

    [Test]
    public void Ancestor_codes_that_are_not_an_array_read_as_none()
    {
        var result = Read(Hit($"{Complete},\"ancestorCodes\":\"01\""), out _);

        result.Results.Single().AncestorCodes.Should().BeEmpty();
    }

    private static PagedResult<CodeListSearchHit> Read(string hit, out int skipped) =>
        Read(hit, null, out skipped);

    private static PagedResult<CodeListSearchHit> Read(string first, string? second, out int skipped)
    {
        var hits = second is null ? first : $"{first},{second}";
        var total = second is null ? 1 : 2;

        using var document = JsonDocument.Parse(
            $"{{\"hits\":{{\"total\":{{\"value\":{total}}},\"hits\":[{hits}]}}}}");

        return CodeListResponseReader.ReadSearch(document.RootElement, page: 1, pageSize: 20, out skipped);
    }

    private static string Hit(string source) => $"{{\"_source\":{{{source}}}}}";
}
