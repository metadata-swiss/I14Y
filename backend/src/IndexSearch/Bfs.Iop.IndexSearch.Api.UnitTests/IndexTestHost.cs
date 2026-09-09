using System.Collections.Concurrent;
using System.Net;
using System.Text;
using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Bfs.Iop.IndexSearch.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

// An IndexController over a stub Elasticsearch, so a test can assert what did and did not reach the
// cluster. A pass now runs detached from the request, so this owns a real container: the orchestrator
// resolves the rebuilders from its own scope, which is the whole point of it having one.
internal sealed class IndexTestHost : IDisposable
{
    private readonly StubHandler _handler = new();
    private readonly HttpClient _client;
    private readonly ServiceProvider _services;
    private readonly bool _ownsGate;

    public IndexTestHost(ReindexGate? gate = null, IndexSearchOptions? options = null)
    {
        _client = new HttpClient(_handler) { BaseAddress = new Uri("http://elasticsearch.test") };

        var names = new IndexNames(Options.Create(new ElasticsearchOptions
        {
            CatalogIndexName = "catalog-index",
            CodeListIndexName = "codelist-index",
        }));

        CatalogSource = Substitute.For<ICatalogDocumentSource>();
        CatalogSource.ReadAllAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Nothing<CatalogIndexDocument>());

        CodeListSource = Substitute.For<ICodeListDocumentSource>();
        CodeListSource.ReadAllAsync(Arg.Any<int>(), Arg.Any<CancellationToken>()).Returns(Nothing<CodeListIndexDocument>());

        var lifetime = Substitute.For<IHostApplicationLifetime>();
        lifetime.ApplicationStopping.Returns(CancellationToken.None);

        var services = new ServiceCollection();

        services.AddLogging();
        services.AddSingleton(Options.Create(options ?? new IndexSearchOptions { ForceMergeAfterReindex = false }));
        services.AddSingleton(names);
        services.AddSingleton(lifetime);
        services.AddScoped<IndexWriteTarget>();

        services.AddScoped(_ => new ElasticsearchIndexProvisioner(
            _client,
            names,
            NullLogger<ElasticsearchIndexProvisioner>.Instance));

        services.AddScoped(_ => new CatalogIndexRebuilder(
            CatalogSource,
            Substitute.For<IDatasetStructureSource>(),
            Substitute.For<ICatalogIndexWriter>(),
            NullLogger<CatalogIndexRebuilder>.Instance));

        services.AddScoped(_ => new CodeListIndexRebuilder(
            CodeListSource,
            Substitute.For<ICodeListIndexWriter>(),
            NullLogger<CodeListIndexRebuilder>.Instance));

        _services = services.BuildServiceProvider();

        // The gate stays out of the container: a borrowed one belongs to the test, and the container
        // disposes every singleton it hands out — including one it was given.
        _ownsGate = gate is null;
        Gate = gate ?? new ReindexGate();

        var orchestrator = new ReindexOrchestrator(
            _services.GetRequiredService<IServiceScopeFactory>(),
            Gate,
            _services.GetRequiredService<IOptions<IndexSearchOptions>>(),
            lifetime,
            _services.GetRequiredService<ILogger<ReindexOrchestrator>>());

        Controller = new IndexController(orchestrator, Gate);
    }

    public ReindexGate Gate { get; }

    public IndexController Controller { get; }

    public ICatalogDocumentSource CatalogSource { get; }

    public ICodeListDocumentSource CodeListSource { get; }

    /// <summary>Every request that reached Elasticsearch, as "METHOD /path".</summary>
    public IReadOnlyList<string> Calls => _handler.Calls;

    /// <summary>Waits for the detached pass to finish, so a test can assert on what it did.</summary>
    public async Task WaitForIdleAsync()
    {
        var deadline = DateTimeOffset.UtcNow.AddSeconds(10);

        while (Gate.IsRunning && DateTimeOffset.UtcNow < deadline)
        {
            await Task.Delay(10);
        }

        Gate.IsRunning.Should().BeFalse("the pass should have finished by now");
    }

    public void Dispose()
    {
        _services.Dispose();
        _client.Dispose();

        if (_ownsGate)
        {
            Gate.Dispose();
        }
    }

    private static async IAsyncEnumerable<IReadOnlyList<T>> Nothing<T>()
    {
        await Task.CompletedTask;

        yield break;
    }

    private sealed class StubHandler : HttpMessageHandler
    {
        private readonly ConcurrentQueue<string> _calls = new();

        // Written from the pass's own thread and read from the test's, so not a List.
        public IReadOnlyList<string> Calls => [.. _calls];

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri!.AbsolutePath;

            _calls.Enqueue(request.Method.Method + " " + path);

            // A cluster that has neither the alias nor a concrete index under its name, which is what
            // a first pass meets. Both answers are 404 there, and the provisioner relies on it.
            if (path.StartsWith("/_alias/", StringComparison.Ordinal) || request.Method == HttpMethod.Head)
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.NotFound));
            }

            // 200 with an empty array, not an empty body: the orphan sweep parses this.
            if (path.StartsWith("/_cat/indices/", StringComparison.Ordinal))
            {
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent("[]", Encoding.UTF8, "application/json"),
                });
            }

            return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK));
        }
    }
}
