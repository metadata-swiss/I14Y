using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class HttpClientBudgetTests
{
    [Test]
    public void A_search_does_not_wait_as_long_as_a_rebuild()
    {
        var factory = Factory();

        var search = factory.CreateClient(ElasticsearchSearchExecutor.HttpClientName).Timeout;
        var bulk = factory.CreateClient(ElasticsearchBulkWriter.HttpClientName).Timeout;
        var admin = factory.CreateClient(ElasticsearchIndexProvisioner.HttpClientName).Timeout;

        // One client for all three meant a user-facing search inherited the bulk budget and hung for
        // two minutes against a stalled cluster, taking the health endpoint with it.
        search.Should().BeLessThan(admin);
        admin.Should().BeLessThan(bulk);

        search.Should().Be(TimeSpan.FromSeconds(5));
        bulk.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Test]
    public void Every_client_points_at_the_configured_node()
    {
        var factory = Factory();

        foreach (var name in new[]
        {
            ElasticsearchSearchExecutor.HttpClientName,
            ElasticsearchBulkWriter.HttpClientName,
            ElasticsearchIndexProvisioner.HttpClientName,
        })
        {
            factory.CreateClient(name).BaseAddress.Should().Be(new Uri("http://elasticsearch.test:9200"));
        }
    }

    private static IHttpClientFactory Factory()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Elasticsearch:Uri"] = "http://elasticsearch.test:9200",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddIndexSearchElasticsearch(configuration);

        return services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>();
    }
}
