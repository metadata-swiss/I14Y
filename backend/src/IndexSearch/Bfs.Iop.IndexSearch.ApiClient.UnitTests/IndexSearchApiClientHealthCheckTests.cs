using AwesomeAssertions;
using Bfs.Iop.IndexSearch.ApiClient.Health;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

[TestFixture]
internal sealed class IndexSearchApiClientHealthCheckTests
{
    [Test]
    public async Task A_service_that_answers_is_healthy()
    {
        var client = Substitute.For<IIndexSearchApiClient>();

        client
            .GetSearchCatalogByQueryAndLanguageAndPageAndPageSizeAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Page());

        var result = await Check(client);

        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Test]
    public async Task A_service_that_throws_is_unhealthy()
    {
        var client = Substitute.For<IIndexSearchApiClient>();

        client
            .GetSearchCatalogByQueryAndLanguageAndPageAndPageSizeAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns<Task<SwaggerResponse<CatalogSearchHitPagedResult>>>(
                _ => throw new HttpRequestException("elasticsearch is unreachable"));

        var result = await Check(client);

        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Exception.Should().BeOfType<HttpRequestException>();
    }

    [Test]
    public async Task The_probe_asks_for_a_single_hit_on_an_anonymous_endpoint()
    {
        // Catalog search, not the status endpoint: status sits behind the rebuild policy, and no
        // token retriever in this solution can produce a token outside a user request, so probing it
        // would report unhealthy forever. One hit keeps the probe cheap while still proving that the
        // request reaches Elasticsearch and comes back.
        var client = Substitute.For<IIndexSearchApiClient>();

        client
            .GetSearchCatalogByQueryAndLanguageAndPageAndPageSizeAsync(
                Arg.Any<string>(), Arg.Any<string>(), Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(Page());

        await Check(client);

        await client.Received(1).GetSearchCatalogByQueryAndLanguageAndPageAndPageSizeAsync(
            query: null,
            language: null,
            page: 1,
            pageSize: 1,
            Arg.Any<CancellationToken>());
    }

    private static Task<HealthCheckResult> Check(IIndexSearchApiClient client) =>
        new IndexSearchApiClientHealthCheck(client)
            .CheckHealthAsync(new HealthCheckContext(), CancellationToken.None);

    private static Task<SwaggerResponse<CatalogSearchHitPagedResult>> Page() =>
        Task.FromResult(new SwaggerResponse<CatalogSearchHitPagedResult>(
            200,
            new Dictionary<string, IEnumerable<string>>(),
            new CatalogSearchHitPagedResult()));
}
