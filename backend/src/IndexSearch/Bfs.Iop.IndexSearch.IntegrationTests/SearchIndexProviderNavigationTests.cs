using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Data.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.IntegrationTests;

[TestFixture]
[Explicit("Needs a populated Postgres.")]
[Category("Pipeline")]
public class SearchIndexProviderNavigationTests
{
    private const int BatchSize = 200;
    private const int MaxCatalogBatches = 25;
    private const int MaxCodeListBatches = 200;

    private ServiceProvider _services = null!;

    [OneTimeSetUp]
    public void Build()
    {
        var postgres = Environment.GetEnvironmentVariable("INDEXSEARCH_TEST_POSTGRES_READONLY");

        if (string.IsNullOrWhiteSpace(postgres))
        {
            Assert.Ignore("INDEXSEARCH_TEST_POSTGRES_READONLY is not set.");
        }

        var configuration = new ConfigurationBuilder().Build();

        var services = new ServiceCollection();

        services.AddLogging();

        services
            .TryAddDataAccessServices(options => options
                .UseNpgsql(postgres)
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration)
            .AddIndexSearchDataServices();

        services.AddScoped(_ => Substitute.For<IUserContextService>());
        services.AddScoped(_ => Substitute.For<IDatasetModelProcessService>());

        _services = services.BuildServiceProvider();
    }

    [OneTimeTearDown]
    public void Release() => _services?.Dispose();

    [Test]
    public async Task A_public_service_arrives_with_its_channels()
    {
        using var scope = _services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<ISearchIndexProviderService>();

        var found = 0;
        var batches = 0;

        await foreach (var batch in provider.GetPublicServicesInBatches(BatchSize))
        {
            found = batch.Count(x => x.Channels.Count > 0);

            if (found > 0 || ++batches >= MaxCatalogBatches)
            {
                break;
            }
        }

        found.Should().BeGreaterThan(
            0,
            "no public service in the first {0} came back with a channel: either "
                + "GetPublicServicesInBatches is missing .Include(d => d.Channels), which silently "
                + "empties ChannelEmails for the whole catalog index, or this database has no channels",
            BatchSize * MaxCatalogBatches);
    }

    [Test]
    public async Task A_public_service_channel_arrives_with_its_email()
    {
        using var scope = _services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<ISearchIndexProviderService>();

        var found = 0;
        var batches = 0;

        await foreach (var batch in provider.GetPublicServicesInBatches(BatchSize))
        {
            found = batch.Count(x => x.Channels.Any(c => !string.IsNullOrWhiteSpace(c.Email)));

            if (found > 0 || ++batches >= MaxCatalogBatches)
            {
                break;
            }
        }

        found.Should().BeGreaterThan(
            0,
            "a public service must be findable by its channel e-mail, and no channel in the first {0} "
                + "carried one",
            BatchSize * MaxCatalogBatches);
    }

    [Test]
    public async Task A_code_list_entry_arrives_with_its_parent_code()
    {
        using var scope = _services.CreateScope();
        var provider = scope.ServiceProvider.GetRequiredService<ISearchIndexProviderService>();

        var found = 0;
        var batches = 0;

        await foreach (var batch in provider.GetCodeListEntriesInBatches(BatchSize))
        {
            found = batch.Count(x => !string.IsNullOrWhiteSpace(x.ParentCode));

            if (found > 0 || ++batches >= MaxCodeListBatches)
            {
                break;
            }
        }

        found.Should().BeGreaterThan(
            0,
            "no code list entry in the first {0} came back with a parent: either "
                + "GetCodeListEntriesInBatches is missing .Include(c => c.ParentCodeListEntry), which "
                + "silently flattens every code list, or this database has no nested entries",
            BatchSize * MaxCodeListBatches);
    }

    [Test]
    public async Task A_code_list_document_carries_its_ancestors()
    {
        using var scope = _services.CreateScope();
        var source = scope.ServiceProvider.GetRequiredService<ICodeListDocumentSource>();

        var found = 0;
        var batches = 0;

        await foreach (var batch in source.ReadAllAsync(BatchSize))
        {
            found = batch.Count(x => x.AncestorCodes.Count > 0);

            if (found > 0 || ++batches >= MaxCodeListBatches)
            {
                break;
            }
        }

        found.Should().BeGreaterThan(
            0,
            "AncestorCodes is what replaced the per-search breadcrumb walk, and nothing in the first "
                + "{0} documents carries one",
            BatchSize * MaxCodeListBatches);
    }
}
