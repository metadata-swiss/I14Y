using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeLists;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class CodeListLanguageTests
{
    private static readonly Guid Concept = Guid.Parse("08dadd0b-a8bb-0855-8607-f4a10a21126f");

    [Test]
    public void Without_a_language_the_query_is_matched_in_every_indexed_language()
    {
        var query = Query("statis", language: null);

        foreach (var language in IndexLanguages.All)
        {
            query.Should().Contain($"\"name.{language}\"");
            query.Should().Contain($"\"name.{language}.ngram\"");
            query.Should().Contain($"\"description.{language}\"");
            query.Should().Contain($"\"annotations.text.{language}\"");
        }
    }

    [Test]
    public void Without_a_language_the_best_matching_language_decides_the_score()
    {
        Query("statis", language: null).Should().Contain("\"dis_max\"");
    }

    [Test]
    public void With_a_language_the_query_is_matched_in_that_language_only()
    {
        var query = Query("statis", language: "en");

        query.Should().Contain("\"name.en\"");
        query.Should().Contain("\"description.en\"");
        query.Should().Contain("\"annotations.text.en\"");
        query.Should().NotContain("\"name.de\"");
        query.Should().NotContain("\"dis_max\"");
    }

    [Test]
    public void A_walk_without_a_language_is_matched_in_every_indexed_language()
    {
        var body = JsonSerializer.Serialize(CodeListQueryBuilder.BuildPointInTimeBody(
            Concept, "statis", language: null, filter: null, DefaultScoring.CodeList,
            pointInTimeId: "pit", keepAlive: TimeSpan.FromMinutes(1), size: 10, searchAfter: null));

        foreach (var language in IndexLanguages.All)
        {
            body.Should().Contain($"\"name.{language}\"");
        }
    }

    private static string Query(string text, string? language) => JsonSerializer.Serialize(
        CodeListQueryBuilder.BuildSearchBody(Concept, text, language, filter: null, DefaultScoring.CodeList, from: 0, size: 10));
}
