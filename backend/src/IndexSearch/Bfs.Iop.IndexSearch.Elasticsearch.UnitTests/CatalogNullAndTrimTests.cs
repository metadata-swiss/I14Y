using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.IndexSearch.Contracts;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
public class CatalogNullAndTrimTests
{
    private static readonly string[] German = ["de"];

    private static IEnumerable<JsonElement> Should(string queryString) =>
        JsonDocument.Parse(JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(queryString, German, null, SearchCaller.Anonymous, 0, 10)))
            .RootElement.GetProperty("query")
            .GetProperty("function_score").GetProperty("query")
            .GetProperty("bool").GetProperty("must")[0].GetProperty("bool")
            .GetProperty("should").EnumerateArray();

    private static IEnumerable<string?> TermValues(string queryString, string field) =>
        Should(queryString)
            .Where(x => x.TryGetProperty("term", out var term) && term.TryGetProperty(field, out _))
            .Select(x => x.GetProperty("term").GetProperty(field).GetString());

    [TestCase("  550e8400-e29b-41d4-a716-446655440000  ")]
    [TestCase("\"550e8400-e29b-41d4-a716-446655440000\"")]
    public void The_id_term_is_built_from_the_trimmed_query(string queryString) =>
        TermValues(queryString, EsCatalogFields.Id)
            .Should().Equal("550e8400-e29b-41d4-a716-446655440000");

    [Test]
    public void The_email_terms_are_built_from_the_trimmed_query() =>
        TermValues("  \"a b\"  ", EsCatalogFields.ContactPointHasEmail)
            .Should().Equal("a b");

    [Test]
    public void A_blank_agency_does_not_widen_what_an_agency_scoped_caller_sees()
    {
        var caller = new SearchCaller
        {
            Role = BusinessRole.Submitter,
            Agencies = ["  ", "BFS"],
        };

        var json = JsonSerializer.Serialize(
            CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, 0, 10));

        json.Should().Contain("\"bfs\"").And.NotContain("\"  \"");
    }

    [Test]
    public void A_null_agency_is_dropped_rather_than_throwing()
    {
        var caller = new SearchCaller
        {
            Role = BusinessRole.LocalDataSteward,
            Agencies = [null!, "BFS"],
        };

        var build = () => CatalogQueryBuilder.BuildSearchBody(null, German, null, caller, 0, 10);

        build.Should().NotThrow();
    }

    [Test]
    public void A_null_channel_email_is_dropped_rather_than_throwing()
    {
        var entry = new CatalogIndexDocument
        {
            Id = Guid.NewGuid(),
            Type = DataAccess.Abstractions.SearchResourceType.Dataset,
            ChannelEmails = [null!, "Info@BFS.ch"],
        };

        var build = () => CatalogDocumentFactory.Build(entry);

        build.Should().NotThrow();

        var (_, doc) = CatalogDocumentFactory.Build(entry);

        doc[EsCatalogFields.ChannelEmail].Should().BeEquivalentTo(new[] { "info@bfs.ch" });
    }
}