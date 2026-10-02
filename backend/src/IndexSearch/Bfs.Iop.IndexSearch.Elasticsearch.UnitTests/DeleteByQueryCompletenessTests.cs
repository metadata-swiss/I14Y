using System.Net;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

/// <summary>
///     A delete by query must not report a partial delete as a complete one.
/// </summary>
/// <remarks>
///     Elasticsearch answers 200 even when it deleted fewer documents than it matched. Documents that
///     changed under the request are skipped and counted in <c>version_conflicts</c> - which
///     <c>conflicts=proceed</c> makes more likely, not less - and per-shard problems land in
///     <c>failures</c> rather than in the status code. Reading only <c>deleted</c> would hide both, the
///     caller would not retry, and the entries that survived would stay searchable until the next
///     nightly rebuild.
/// </remarks>
[TestFixture]
internal sealed class DeleteByQueryCompletenessTests
{
    [Test]
    public async Task A_complete_delete_reports_what_it_removed()
    {
        var writer = Writer("""{"total":119,"deleted":119,"version_conflicts":0,"failures":[]}""");

        var deleted = await writer.DeleteByQueryAsync("i14y-codelist", MatchAll(), CancellationToken.None);

        deleted.Should().Be(119);
    }

    [Test]
    public async Task A_delete_that_hit_version_conflicts_is_not_success()
    {
        // conflicts=proceed skips the conflicted documents and carries on, so they are still there.
        var writer = Writer("""{"total":119,"deleted":117,"version_conflicts":2,"failures":[]}""");

        var delete = async () => await writer.DeleteByQueryAsync("i14y-codelist", MatchAll(), CancellationToken.None);

        (await delete.Should().ThrowAsync<HttpRequestException>())
            .WithMessage("*117 of 119*2 version conflicts*");
    }

    [Test]
    public async Task A_delete_with_shard_failures_is_not_success()
    {
        var writer = Writer(
            """{"total":10,"deleted":10,"version_conflicts":0,"failures":[{"index":"i14y-codelist","status":500}]}""");

        var delete = async () => await writer.DeleteByQueryAsync("i14y-codelist", MatchAll(), CancellationToken.None);

        (await delete.Should().ThrowAsync<HttpRequestException>()).WithMessage("*1 failures*");
    }

    [Test]
    public async Task A_delete_that_removed_less_than_it_matched_is_not_success()
    {
        // Neither counter explains the shortfall, so nothing above would catch this on its own.
        var writer = Writer("""{"total":119,"deleted":100,"version_conflicts":0,"failures":[]}""");

        var delete = async () => await writer.DeleteByQueryAsync("i14y-codelist", MatchAll(), CancellationToken.None);

        (await delete.Should().ThrowAsync<HttpRequestException>()).WithMessage("*100 of 119*");
    }

    private static Dictionary<string, object?> MatchAll() => new()
    {
        ["match_all"] = new Dictionary<string, object?>(),
    };

    private static ElasticsearchBulkWriter Writer(string payload) =>
        new(
            new HttpClient(new StubHandler(payload)) { BaseAddress = new Uri("http://elasticsearch.test") },
            NullLogger<ElasticsearchBulkWriter>.Instance);

    private sealed class StubHandler(string payload) : HttpMessageHandler
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
