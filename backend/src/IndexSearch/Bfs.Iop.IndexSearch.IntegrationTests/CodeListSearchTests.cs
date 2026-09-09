using AwesomeAssertions;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Contracts.Search;
using Bfs.Iop.IndexSearch.Data.Extensions;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Bfs.Iop.IndexSearch.Elasticsearch.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.IntegrationTests;

// Searching a code list needs both backends at once: the hits come from Elasticsearch and the
// breadcrumbs from Postgres, and the join between them is the thing no single-backend test can check.
//
//   INDEXSEARCH_TEST_POSTGRES_READONLY   a database with code lists in it
//   INDEXSEARCH_TEST_ELASTICSEARCH       defaults to the local node
[TestFixture]
[Explicit("Needs a populated Postgres and a live Elasticsearch, and a rebuild to have run.")]
[Category("Pipeline")]
public class CodeListSearchTests
{
    private const string CatalogIndex = "i14y-catalog-pipelinetest";
    private const string CodeListIndex = "i14y-codelist-pipelinetest";

    private ServiceProvider _provider = null!;
    private Guid _conceptId;
    private string _code = string.Empty;

    [OneTimeSetUp]
    public async Task BuildAsync()
    {
        var postgres = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_POSTGRES_READONLY");

        if (string.IsNullOrWhiteSpace(postgres))
        {
            Assert.Ignore("INDEXSEARCH_TEST_POSTGRES_READONLY is not set.");
        }

        var elasticUri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH")
            ?? "http://localhost:9200";

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Elasticsearch:Uri"] = elasticUri,
                ["Elasticsearch:CatalogIndexName"] = CatalogIndex,
                ["Elasticsearch:CodeListIndexName"] = CodeListIndex,
            })
            .Build();

        var services = new ServiceCollection();

        services.AddLogging();

        services
            .TryAddDataAccessServices(options => options
                .UseNpgsql(postgres)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration)
            .AddIndexSearchDataServices()
            .AddIndexSearchElasticsearch(configuration);

        services.AddScoped(_ => Substitute.For<IUserContextService>());
        services.AddScoped(_ => Substitute.For<IDatasetModelProcessService>());

        _provider = services.BuildServiceProvider();

        // Seeded from the index rather than the database: IopDbContext is internal to Core.Data and
        // only the adapter project may see it, which is the boundary this whole design rests on.
        using var client = new HttpClient { BaseAddress = new Uri(elasticUri) };

        var body = new Dictionary<string, object?>
        {
            ["size"] = 1,
            // An entry with a parent, so the breadcrumb has more than one level to prove.
            ["query"] = new Dictionary<string, object?>
            {
                ["exists"] = new Dictionary<string, object?> { ["field"] = "parentCode" },
            },
        };

        var response = await client.PostAsJsonAsync($"/{CodeListIndex}/_search", body);

        if (!response.IsSuccessStatusCode)
        {
            Assert.Ignore($"No usable {CodeListIndex} index; run the rebuild pipeline test first.");
        }

        var hits = JsonDocument.Parse(await response.Content.ReadAsStringAsync())
            .RootElement.GetProperty("hits").GetProperty("hits");

        if (hits.GetArrayLength() == 0)
        {
            Assert.Ignore($"{CodeListIndex} is empty; run the rebuild pipeline test first.");
        }

        var source = hits[0].GetProperty("_source");

        _conceptId = Guid.Parse(source.GetProperty("conceptId").GetString()!);
        _code = source.GetProperty("code").GetString()!;
    }

    [OneTimeTearDown]
    public void Dispose() => _provider?.Dispose();

    [Test]
    public async Task A_hit_carries_its_ancestors_without_a_database_round_trip()
    {
        using var scope = _provider.CreateScope();

        var result = await Service(scope).SearchAsync(
            _conceptId, _code, "de", filter: null, page: 1, pageSize: 10);

        result.Results.Should().NotBeEmpty("the entry was searched for by its own code");

        var hit = result.Results.First(x => x.Code == _code);

        // Written into the document at index time, where the parent map is already in memory, rather
        // than walked per search. The engine is the only dependency this test resolves.
        hit.AncestorCodes.Should().NotBeEmpty();

        // Nearest parent first, and the entry itself is not one of its own ancestors.
        hit.AncestorCodes[0].Should().Be(hit.ParentCode);
        hit.AncestorCodes.Should().NotContain(hit.Code);
    }

    [Test]
    public async Task A_search_is_confined_to_the_concept_it_names()
    {
        using var scope = _provider.CreateScope();

        var result = await scope.ServiceProvider.GetRequiredService<ICodeListSearchEngine>()
            .SearchAsync(_conceptId, query: null, "de", filter: null, page: 1, pageSize: 50);

        result.Results.Should().NotBeEmpty();
        result.Results.Should().AllSatisfy(
            x => x.ConceptId.Should().Be(_conceptId, "a code list is never searched across concepts"));
    }

    private static ICodeListSearchEngine Service(IServiceScope scope) =>
        scope.ServiceProvider.GetRequiredService<ICodeListSearchEngine>();
}
