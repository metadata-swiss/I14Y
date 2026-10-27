using System.Net;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class DeleteByQueryCompletenessTests
{
    [Test]
    public async Task A_delete_that_removed_everything_it_matched_reports_the_count()
    {
        var writer = WriterAnswering(RespondedWith(matched: 119, deleted: 119));

        var deleted = await DeleteAsync(writer);

        deleted.Should().Be(119);
    }

    [Test]
    public async Task A_delete_that_removed_fewer_than_it_matched_throws_despite_the_200()
    {
        var writer = WriterAnswering(RespondedWith(matched: 119, deleted: 100));

        var delete = () => DeleteAsync(writer);

        (await delete.Should().ThrowAsync<HttpRequestException>()).WithMessage("*100 of 119*");
    }

    [Test]
    public async Task A_shortfall_names_the_version_conflicts_that_caused_it()
    {
        var writer = WriterAnswering(RespondedWith(matched: 119, deleted: 117, versionConflicts: 2));

        var delete = () => DeleteAsync(writer);

        (await delete.Should().ThrowAsync<HttpRequestException>())
            .WithMessage("*117 of 119*2 version conflicts*");
    }

    [Test]
    public async Task A_delete_reporting_shard_failures_throws_even_though_it_matched_everything()
    {
        var writer = WriterAnswering(RespondedWith(matched: 10, deleted: 10, shardFailures: 1));

        var delete = () => DeleteAsync(writer);

        (await delete.Should().ThrowAsync<HttpRequestException>()).WithMessage("*1 failures*");
    }

    [Test]
    public async Task A_timed_out_delete_throws_even_though_it_matched_everything()
    {
        var writer = WriterAnswering(RespondedWith(matched: 10, deleted: 10, timedOut: true));

        var delete = () => DeleteAsync(writer);

        (await delete.Should().ThrowAsync<HttpRequestException>()).WithMessage("*timed out: True*");
    }

    private static Task<int> DeleteAsync(ElasticsearchBulkWriter writer) =>
        writer.DeleteByQueryAsync("i14y-codelist", MatchAll(), CancellationToken.None);

    private static string RespondedWith(
        int matched,
        int deleted,
        int versionConflicts = 0,
        int shardFailures = 0,
        bool timedOut = false)
    {
        var failures = string.Join(
            ",",
            Enumerable.Repeat("""{"index":"i14y-codelist","status":500}""", shardFailures));

        return $$"""
            {
              "total": {{matched}},
              "deleted": {{deleted}},
              "version_conflicts": {{versionConflicts}},
              "timed_out": {{(timedOut ? "true" : "false")}},
              "failures": [{{failures}}]
            }
            """;
    }

    private static Dictionary<string, object?> MatchAll() => new()
    {
        ["match_all"] = new Dictionary<string, object?>(),
    };

    private static ElasticsearchBulkWriter WriterAnswering(string payload) =>
        new(
            new HttpClient(new AlwaysAnswers(payload)) { BaseAddress = new Uri("http://elasticsearch.test") },
            NullLogger<ElasticsearchBulkWriter>.Instance);

    private sealed class AlwaysAnswers(string payload) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(payload, Encoding.UTF8, "application/json"),
            });
    }
}
