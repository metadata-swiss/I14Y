using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// The code list is the larger index by far and its hits carry the most structure — ancestors and
// nested annotations — so this pins how a stored entry becomes the model the API returns.
[TestFixture]
internal sealed class CodeListResponseReaderTests
{
    private const string Id = "11111111-1111-1111-1111-111111111111";
    private const string ConceptId = "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa";

    private static readonly string Complete = $"\"id\":\"{Id}\",\"conceptId\":\"{ConceptId}\",\"code\":\"01.02\"";

    [Test]
    public void An_entry_is_mapped_from_the_document_source()
    {
        var hit = Read(Complete).Results.Single();

        hit.Id.Should().Be(Guid.Parse(Id));
        hit.ConceptId.Should().Be(Guid.Parse(ConceptId));
        hit.Code.Should().Be("01.02");
    }

    [Test]
    public void The_ancestors_the_rebuild_flattened_are_read_back_in_order()
    {
        // The breadcrumb the front end renders, nearest first, exactly as the document stores it.
        Read($"{Complete},\"ancestorCodes\":[\"01.02\",\"01\"]").Results.Single()
            .AncestorCodes.Should().Equal("01.02", "01");
    }

    [Test]
    public void Annotations_are_read_with_their_text()
    {
        var annotations = Read(
            $"{Complete},\"annotations\":[{{\"type\":\"note\",\"title\":\"Erste\",\"text\":{{\"de\":\"Achtung\"}}}}]")
            .Results.Single().Annotations;

        annotations.Should().ContainSingle();
        annotations[0].Type.Should().Be("note");
        annotations[0].Text!.De.Should().Be("Achtung");
    }

    [Test]
    public void An_entry_without_annotations_reads_as_none_rather_than_null()
    {
        // The contract hands the front end a list to iterate; null would make every caller guard.
        Read(Complete).Results.Single().Annotations.Should().BeEmpty();
    }

    [Test]
    public void A_name_whose_languages_are_all_blank_reads_as_absent()
    {
        // The model treats whitespace as no content, so an empty label falls back instead of rendering.
        Read($"{Complete},\"name\":{{\"de\":\"  \"}}").Results.Single().Name.Should().BeNull();
    }

    [Test]
    public void The_total_comes_from_the_server_and_not_from_the_rows_returned()
    {
        Read(Complete, total: 24_726).TotalCount.Should().Be(24_726);
    }

    private static PagedResult<CodeListSearchHit> Read(string source, int total = 1)
    {
        using var document = JsonDocument.Parse(
            $"{{\"hits\":{{\"total\":{{\"value\":{total}}},\"hits\":[{{\"_source\":{{{source}}}}}]}}}}");

        return CodeListResponseReader.ReadSearch(document.RootElement, page: 1, pageSize: 20);
    }
}
