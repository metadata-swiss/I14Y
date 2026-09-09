using System.Diagnostics;
using System.Net.Http;
using System.Text.Json;
using AwesomeAssertions;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Business;
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

// Every layer has been checked on its own — the reads against Npgsql, the mapping against fixtures,
// the writers and the mappings against a real Elasticsearch. None of that says the chain works when
// joined up, and nothing measures it. This runs the real rebuild through the real dependency graph.
//
// Reads Postgres and never writes to it, so it is safe to point at a restored copy:
//   INDEXSEARCH_TEST_POSTGRES_READONLY   a database with data in it
//   INDEXSEARCH_TEST_ELASTICSEARCH       defaults to the local node
[TestFixture]
[Explicit("Needs a populated Postgres and a live Elasticsearch.")]
[Category("Pipeline")]
public class RebuildPipelineTests
{
    private const string CatalogIndex = "i14y-catalog-pipelinetest";
    private const string CodeListIndex = "i14y-codelist-pipelinetest";

    private ServiceProvider _provider = null!;
    private HttpClient _elastic = null!;

    [OneTimeSetUp]
    public void Build()
    {
        var postgres = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_POSTGRES_READONLY");

        if (string.IsNullOrWhiteSpace(postgres))
        {
            Assert.Ignore("INDEXSEARCH_TEST_POSTGRES_READONLY is not set.");
        }

        var elasticUri = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_ELASTICSEARCH")
            ?? "http://localhost:9200";

        if (new Uri(elasticUri).Host is not ("localhost" or "127.0.0.1" or "[::1]"))
        {
            Assert.Fail("Refusing to run: this fixture drops Elasticsearch indices. Use a local node.");
        }

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

        // The same registrations a host would make, so the dependency graph itself is under test.
        services
            .TryAddDataAccessServices(options => options
                .UseNpgsql(postgres)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration)
            .AddIndexSearchDataServices()
            .AddIndexSearchElasticsearch(configuration);

        services.AddScoped(_ => Substitute.For<IUserContextService>());

        // Structures live in an object store that is not part of this measurement. An empty set means
        // datasets are indexed with the flag set to false rather than omitted, which is the document
        // shape a real rebuild produces.
        var structures = Substitute.For<IDatasetModelProcessService>();
        structures.GetAllDatasetIdsWithStructures(Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<IEnumerable<string>>([]));

        services.AddScoped(_ => structures);

        _provider = services.BuildServiceProvider();
        _elastic = new HttpClient { BaseAddress = new Uri(elasticUri), Timeout = TimeSpan.FromMinutes(2) };
    }

    [OneTimeTearDown]
    public void Dispose()
    {
        _provider?.Dispose();
        _elastic?.Dispose();
    }

    [Test]
    public async Task The_whole_catalog_is_read_mapped_and_indexed()
    {
        using var scope = _provider.CreateScope();

        var provisioner = scope.ServiceProvider.GetRequiredService<ElasticsearchIndexProvisioner>();

        var prepared = await provisioner.PrepareAsync();

        scope.ServiceProvider.GetRequiredService<IndexWriteTarget>()
            .RedirectTo(prepared.Catalog, prepared.CodeList);

        await provisioner.PublishAsync(prepared);

        var rebuilder = scope.ServiceProvider.GetRequiredService<CatalogIndexRebuilder>();

        var started = Stopwatch.StartNew();
        var report = await rebuilder.RebuildAsync(batchSize: 500);
        started.Stop();

        await Report("catalog", report, started.Elapsed, CatalogIndex);

        report.DocumentsWritten.Should().Be(report.DocumentsSent, "the index should accept every document");
        report.BatchesFailed.Should().Be(0);
        report.DocumentsWritten.Should().BeGreaterThan(0);

        (await CountAsync(CatalogIndex)).Should().Be(report.DocumentsWritten);
    }

    [Test]
    public async Task The_whole_code_list_is_read_mapped_and_indexed()
    {
        using var scope = _provider.CreateScope();

        var rebuilder = scope.ServiceProvider.GetRequiredService<CodeListIndexRebuilder>();

        var started = Stopwatch.StartNew();
        var report = await rebuilder.RebuildAsync(batchSize: 1000);
        started.Stop();

        await Report("code list", report, started.Elapsed, CodeListIndex);

        report.DocumentsWritten.Should().Be(report.DocumentsSent, "the index should accept every entry");
        report.BatchesFailed.Should().Be(0);
        report.DocumentsWritten.Should().BeGreaterThan(0);
    }

    private async Task Report(string what, IndexRebuildReport report, TimeSpan elapsed, string index)
    {
        var perSecond = elapsed.TotalSeconds > 0
            ? report.DocumentsWritten / elapsed.TotalSeconds
            : 0;

        await TestContext.Out.WriteLineAsync(
            $"{what}: {report.DocumentsWritten:N0} of {report.DocumentsSent:N0} documents in "
            + $"{elapsed:mm\\:ss\\.f} ({perSecond:N0}/s), {report.BatchesFailed} batches failed, "
            + $"index {await SizeAsync(index)}");
    }

    private async Task<int> CountAsync(string index)
    {
        await _elastic.PostAsync($"/{index}/_refresh", content: null);

        var payload = await _elastic.GetStringAsync($"/{index}/_count");

        return JsonDocument.Parse(payload).RootElement.GetProperty("count").GetInt32();
    }

    private async Task<string> SizeAsync(string index)
    {
        await _elastic.PostAsync($"/{index}/_refresh", content: null);

        var payload = await _elastic.GetStringAsync($"/{index}/_stats/store");

        var bytes = JsonDocument.Parse(payload).RootElement
            .GetProperty("_all").GetProperty("primaries").GetProperty("store")
            .GetProperty("size_in_bytes").GetInt64();

        return $"{bytes / 1024d / 1024d:N1} MB";
    }
}
