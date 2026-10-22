using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.Extensions;
using Microsoft.Extensions.Configuration;
using System.Reflection;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture]
internal sealed class HttpClientLimitsTests
{
    [Test]
    public void A_search_does_not_wait_as_long_as_a_rebuild()
    {
        var factory = Factory();

        var search = factory.CreateClient(ElasticsearchSearchExecutor.HttpClientName).Timeout;
        var bulk = factory.CreateClient(ElasticsearchBulkWriter.HttpClientName).Timeout;
        var admin = factory.CreateClient(ElasticsearchIndexProvisioner.HttpClientName).Timeout;

        // One client for all three meant a user-facing search inherited the bulk timeout and hung for
        // two minutes against a stalled cluster, taking the health endpoint with it.
        search.Should().BeLessThan(admin);
        admin.Should().BeLessThan(bulk);

        search.Should().Be(TimeSpan.FromSeconds(5));
        bulk.Should().Be(TimeSpan.FromMinutes(2));
    }

    [Test]
    public void Reading_a_value_back_to_rewrite_it_outlasts_and_outlives_a_search()
    {
        var carryForward = Limits(ElasticsearchCatalogIndexReader.HttpClientName);
        var search = Limits(ElasticsearchSearchExecutor.HttpClientName);

        // Sharing the search client meant this read gave up after 5s and two attempts. Nobody is
        // waiting on it, and giving up is not free: the value it reads is about to be written, so a
        // failed read deletes the indexed one until the next rebuild instead of just delaying it.
        carryForward.Timeout.Should().BeGreaterThan(search.Timeout);
        carryForward.Attempts.Should().BeGreaterThan(search.Attempts);
    }

    [Test]
    public void The_index_reader_is_the_one_given_those_limits()
    {
        var services = Services().BuildServiceProvider();

        // Asserting the registration alone proved nothing: pointing the reader back at the search
        // client left every limit correct and the fix undone.
        var client = services.GetRequiredService<ICatalogIndexReader>()
            .GetType()
            .GetField("_client", BindingFlags.Instance | BindingFlags.NonPublic)!
            .GetValue(services.GetRequiredService<ICatalogIndexReader>()) as HttpClient;

        client!.Timeout.Should().Be(Limits(ElasticsearchCatalogIndexReader.HttpClientName).Timeout);
    }

    private static HttpClientLimits Limits(string name) =>
        ServiceCollectionExtensions.ClientLimits.Single(x => x.Name == name);

    [Test]
    public void Every_client_points_at_the_configured_node()
    {
        var factory = Factory();

        foreach (var name in new[]
        {
            ElasticsearchSearchExecutor.HttpClientName,
            ElasticsearchBulkWriter.HttpClientName,
            ElasticsearchIndexProvisioner.HttpClientName,
            ElasticsearchCatalogIndexReader.HttpClientName,
        })
        {
            factory.CreateClient(name).BaseAddress.Should().Be(new Uri("http://elasticsearch.test:9200"));
        }
    }

    private static IHttpClientFactory Factory() =>
        Services().BuildServiceProvider().GetRequiredService<IHttpClientFactory>();

    private static IServiceCollection Services()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Elasticsearch:Uri"] = "http://elasticsearch.test:9200",
                ["Elasticsearch:CatalogIndexName"] = "i14y-catalog",
                ["Elasticsearch:CodeListIndexName"] = "i14y-codelist",
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddIndexSearchElasticsearch(configuration);

        return services;
    }
}
