using System.Net;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class OrphanSweepTests
{
    private const string Alias = "i14y-catalog";

    [Test]
    public async Task An_index_a_pass_never_published_is_dropped()
    {
        var host = Host(members: [$"{Alias}-20260909120000000"], indices: [$"{Alias}-20260101000000000"]);

        await host.Provisioner.SweepOrphansAsync();

        host.Deletes.Should().Contain($"{Alias}-20260101000000000");
    }

    [Test]
    public async Task The_index_the_alias_serves_is_never_dropped()
    {
        var live = $"{Alias}-20260909120000000";

        var host = Host(members: [live], indices: [live]);

        await host.Provisioner.SweepOrphansAsync();

        host.Deletes.Should().NotContain(live);
    }

    [TestCase("i14y-catalog-roundtriptest")]
    [TestCase("i14y-catalog-contracttest")]
    [TestCase("i14y-catalog-writertest")]
    [TestCase("i14y-catalog-archive")]
    [TestCase("i14y-catalog-2026")]
    public async Task An_index_that_only_shares_the_prefix_is_left_alone(string neighbour)
    {
        var host = Host(members: [], indices: [neighbour]);

        await host.Provisioner.SweepOrphansAsync();

        // A prefix match alone would take the test fixtures and anything else parked under the name.
        host.Deletes.Should().NotContain(neighbour);
    }

    [Test]
    public async Task A_generation_young_enough_to_still_be_filling_is_left_alone()
    {
        // No alias yet is what a half-finished rebuild looks like from the outside, and another
        // instance's is indistinguishable from one this instance abandoned. Deleting it does not fail
        // that rebuild either: its next bulk write recreates the index with a default mapping, and it
        // publishes that.
        var host = Host(
            members: [],
            indices: [$"{Alias}-20260910115900000"],
            now: new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));

        await host.Provisioner.SweepOrphansAsync();

        host.Deletes.Should().BeEmpty();
    }

    [Test]
    public async Task A_generation_no_pass_could_still_be_filling_is_dropped()
    {
        // The positive control for the window above: past it, an unaliased generation is what the
        // sweep exists for, and leaving it would keep an entire copy of the index on disk.
        var host = Host(
            members: [],
            indices: [$"{Alias}-20260910105900000"],
            now: new DateTimeOffset(2026, 9, 10, 12, 0, 0, TimeSpan.Zero));

        await host.Provisioner.SweepOrphansAsync();

        host.Deletes.Should().Contain($"{Alias}-20260910105900000");
    }

    private static TestHost Host(string[] members, string[] indices, DateTimeOffset? now = null)
    {
        var handler = new StubHandler(members, indices);

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://elasticsearch.test") };

        var names = new IndexNames(Options.Create(new ElasticsearchOptions
        {
            CatalogIndexName = Alias,
            CodeListIndexName = "i14y-codelist",
        }));

        return new TestHost(
            new ElasticsearchIndexProvisioner(
                client,
                names,
                NullLogger<ElasticsearchIndexProvisioner>.Instance,
                now is null ? null : new FixedClock(now.Value)),
            handler.Deletes);
    }

    private sealed record TestHost(ElasticsearchIndexProvisioner Provisioner, IReadOnlyList<string> Deletes);

    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    private sealed class StubHandler(string[] members, string[] indices) : HttpMessageHandler
    {
        private readonly List<string> _deletes = [];

        public IReadOnlyList<string> Deletes => _deletes;

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;

            if (request.Method == HttpMethod.Delete)
            {
                _deletes.Add(path.TrimStart('/'));

                return Json("{}");
            }

            if (path.StartsWith("/_alias/", StringComparison.Ordinal))
            {
                var owner = path["/_alias/".Length..];

                var mine = members.Where(x => x.StartsWith($"{owner}-", StringComparison.Ordinal)).ToArray();

                return mine.Length == 0
                    ? Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound))
                    : Json("{" + string.Join(",", mine.Select(x => $"\"{x}\":{{}}")) + "}");
            }

            if (path.StartsWith("/_cat/indices/", StringComparison.Ordinal))
            {
                var pattern = path["/_cat/indices/".Length..].TrimEnd('*');

                var matching = indices
                    .Concat(members)
                    .Distinct(StringComparer.Ordinal)
                    .Where(x => x.StartsWith(pattern, StringComparison.Ordinal));

                return Json("[" + string.Join(",", matching.Select(x => $"{{\"index\":\"{x}\"}}")) + "]");
            }

            return Json("{}");
        }

        private static Task<HttpResponseMessage> Json(string body) =>
            Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(body, Encoding.UTF8, "application/json"),
            });
    }
}
