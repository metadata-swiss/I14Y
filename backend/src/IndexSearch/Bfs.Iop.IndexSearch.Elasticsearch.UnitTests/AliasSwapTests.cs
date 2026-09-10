using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

// Publishing is the data-safety boundary of a rebuild. Both aliases have to move together, and
// nothing may be deleted until they have: one request per alias left a new catalog paired with the
// previous code list, and the superseded catalog already dropped, so there was no consistent pair to
// fall back to.
[TestFixture]
internal sealed class AliasSwapTests
{
    private const string Catalog = "i14y-catalog";
    private const string CodeList = "i14y-codelist";

    private static readonly PreparedIndices Prepared =
        new($"{Catalog}-20260910120000000", $"{CodeList}-20260910120000000");

    [Test]
    public async Task Both_aliases_move_in_one_request()
    {
        var host = Host(members: [$"{Catalog}-20260101000000000", $"{CodeList}-20260101000000000"]);

        await host.Provisioner.PublishAsync(Prepared);

        host.Calls.Count(x => x == "POST /_aliases").Should().Be(1);

        var actions = host.AliasActions.Single();

        actions.Should().Contain(x => x.Alias == Catalog && x.Index == Prepared.Catalog && x.Kind == "add");
        actions.Should().Contain(x => x.Alias == CodeList && x.Index == Prepared.CodeList && x.Kind == "add");
        actions.Should().Contain(x => x.Kind == "remove" && x.Alias == Catalog);
        actions.Should().Contain(x => x.Kind == "remove" && x.Alias == CodeList);
    }

    [Test]
    public async Task A_failed_swap_leaves_the_superseded_indices_in_place()
    {
        var previous = $"{Catalog}-20260101000000000";

        var host = Host(members: [previous, $"{CodeList}-20260101000000000"], aliasesFails: true);

        var publish = async () => await host.Provisioner.PublishAsync(Prepared);

        await publish.Should().ThrowAsync<HttpRequestException>();

        // The rollback copy. Deleting it before the swap is confirmed would leave nothing to serve
        // from and nothing to go back to.
        host.Calls.Should().NotContain($"DELETE /{previous}");
    }

    [Test]
    public async Task A_successful_swap_drops_what_the_aliases_pointed_at_before()
    {
        var previous = $"{Catalog}-20260101000000000";

        var host = Host(members: [previous, $"{CodeList}-20260101000000000"]);

        await host.Provisioner.PublishAsync(Prepared);

        // The positive control: without it the failure test above would pass even if the code never
        // deleted anything at all.
        host.Calls.Should().Contain($"DELETE /{previous}");
    }

    [Test]
    public async Task The_deletions_only_start_once_every_alias_has_moved()
    {
        var host = Host(members: [$"{Catalog}-20260101000000000", $"{CodeList}-20260101000000000"]);

        await host.Provisioner.PublishAsync(Prepared);

        var swap = host.Calls.ToList().IndexOf("POST /_aliases");
        var firstDelete = host.Calls.ToList().FindIndex(x => x.StartsWith("DELETE /", StringComparison.Ordinal));

        firstDelete.Should().BeGreaterThan(swap);
    }

    [Test]
    public async Task A_concrete_index_holding_the_alias_name_is_replaced_inside_the_transaction()
    {
        // An alias cannot share a name with an index, so the first swap on a cluster that still has
        // the concrete index has to remove it. Doing that as its own request would open a window with
        // neither the index nor the alias present.
        var host = Host(members: [], concreteIndexExists: true);

        await host.Provisioner.PublishAsync(Prepared);

        host.AliasActions.Single().Should().Contain(x => x.Kind == "remove_index" && x.Index == Catalog);
        host.Calls.Should().NotContain($"DELETE /{Catalog}");
    }

    [Test]
    public async Task An_alias_is_cleared_of_every_generation_and_not_only_the_ones_read_back()
    {
        // The publish an instance makes while another is mid-bootstrap. Both read an alias that
        // resolves to nothing, so add-only actions would leave it holding both generations: every
        // document would answer twice, and a write through the alias would fail for having no write
        // index. Removing by pattern makes the last publish the only one that counts.
        var host = Host(members: []);

        await host.Provisioner.PublishAsync(Prepared);

        var actions = host.AliasActions.Single();

        actions.Should().Contain(x => x.Kind == "remove" && x.Alias == Catalog && x.Index == $"{Catalog}-*");
        actions.Should().Contain(x => x.Kind == "remove" && x.Alias == CodeList && x.Index == $"{CodeList}-*");
    }

    [Test]
    public async Task A_pattern_that_matches_no_alias_yet_is_not_a_failed_publish()
    {
        // What that removal answers on an empty cluster: 200 for the request, a 404 on the action, and
        // the add applied all the same.
        var host = Host(members: [], actionError: "aliases_not_found_exception");

        var publish = async () => await host.Provisioner.PublishAsync(Prepared);

        await publish.Should().NotThrowAsync();
    }

    [Test]
    public async Task An_action_the_server_rejected_fails_the_publish()
    {
        // The positive control, and the reason the body is read at all: Elasticsearch reports a
        // rejected action inside a 200, so the status line alone would call this swap a success.
        var host = Host(members: [], actionError: "index_not_found_exception");

        var publish = async () => await host.Provisioner.PublishAsync(Prepared);

        await publish.Should().ThrowAsync<HttpRequestException>();
    }

    [Test]
    public async Task A_success_with_nothing_to_read_is_still_a_success()
    {
        // The status line is what says the swap happened. Reading the body adds detail, and a body
        // that carries none must not turn a swap the server accepted into a failed pass.
        var host = Host(members: [], emptyBody: true);

        var publish = async () => await host.Provisioner.PublishAsync(Prepared);

        await publish.Should().NotThrowAsync();
    }

    private static TestHost Host(
        string[] members,
        bool aliasesFails = false,
        bool concreteIndexExists = false,
        string? actionError = null,
        bool emptyBody = false)
    {
        var handler = new StubHandler(members, aliasesFails, concreteIndexExists, actionError, emptyBody);

        var client = new HttpClient(handler) { BaseAddress = new Uri("http://elasticsearch.test") };

        var names = new IndexNames(Options.Create(new ElasticsearchOptions
        {
            CatalogIndexName = Catalog,
            CodeListIndexName = CodeList,
        }));

        return new TestHost(
            new ElasticsearchIndexProvisioner(client, names, NullLogger<ElasticsearchIndexProvisioner>.Instance),
            handler);
    }

    private sealed record Action(string Kind, string? Alias, string? Index);

    // The handler itself, not a snapshot of its lists: taking those at construction captured them
    // while still empty, and a NotContain assertion against an empty list passes for the wrong reason.
    private sealed record TestHost(ElasticsearchIndexProvisioner Provisioner, StubHandler Handler)
    {
        public IReadOnlyList<string> Calls => Handler.Calls;

        public IReadOnlyList<IReadOnlyList<Action>> AliasActions => Handler.AliasActions;
    }

    private sealed class StubHandler(
        string[] members,
        bool aliasesFails,
        bool concreteIndexExists,
        string? actionError,
        bool emptyBody)
        : HttpMessageHandler
    {
        private readonly ConcurrentQueue<string> _calls = new();
        private readonly List<IReadOnlyList<Action>> _aliasActions = [];

        public IReadOnlyList<string> Calls => [.. _calls];

        public IReadOnlyList<IReadOnlyList<Action>> AliasActions => _aliasActions;

        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;

            _calls.Enqueue($"{request.Method.Method} {path}");

            if (path == "/_aliases")
            {
                _aliasActions.Add(Parse(await request.Content!.ReadAsStringAsync(cancellationToken)));

                // The shape Elasticsearch answers a partly rejected transaction with: 200, errors true,
                // and the verdict on each action.
                var payload = actionError is null
                    ? "{\"acknowledged\":true,\"errors\":false}"
                    : "{\"acknowledged\":true,\"errors\":true,\"action_results\":[{\"status\":404,"
                        + $"\"error\":{{\"type\":\"{actionError}\"}}}},{{\"status\":200}}]}}";

                var status = aliasesFails ? HttpStatusCode.InternalServerError : HttpStatusCode.OK;

                return emptyBody
                    ? new HttpResponseMessage(status)
                    : new HttpResponseMessage(status)
                    {
                        Content = new StringContent(payload, Encoding.UTF8, "application/json"),
                    };
            }

            if (path.StartsWith("/_alias/", StringComparison.Ordinal))
            {
                var owner = path["/_alias/".Length..];

                var mine = members.Where(x => x.StartsWith($"{owner}-", StringComparison.Ordinal)).ToArray();

                return mine.Length == 0
                    ? new HttpResponseMessage(HttpStatusCode.NotFound)
                    : Json("{" + string.Join(",", mine.Select(x => $"\"{x}\":{{}}")) + "}");
            }

            if (request.Method == HttpMethod.Head)
            {
                return new HttpResponseMessage(
                    concreteIndexExists ? HttpStatusCode.OK : HttpStatusCode.NotFound);
            }

            return Json("{}");
        }

        private static IReadOnlyList<Action> Parse(string body)
        {
            using var document = JsonDocument.Parse(body);

            return
            [
                .. document.RootElement.GetProperty("actions").EnumerateArray().Select(x =>
                {
                    var action = x.EnumerateObject().Single();

                    return new Action(
                        action.Name,
                        action.Value.TryGetProperty("alias", out var alias) ? alias.GetString() : null,
                        action.Value.TryGetProperty("index", out var index) ? index.GetString() : null);
                }),
            ];
        }

        private static HttpResponseMessage Json(string body) =>
            new(HttpStatusCode.OK) { Content = new StringContent(body, Encoding.UTF8, "application/json") };
    }
}
