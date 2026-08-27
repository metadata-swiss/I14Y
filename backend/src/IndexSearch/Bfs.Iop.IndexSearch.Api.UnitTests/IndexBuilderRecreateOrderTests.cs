using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Bfs.Iop.Search.Abstractions;
using Bfs.Iop.Search.Elasticsearch;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Pins the wiring between the drop and the readiness latch.
/// <para>
/// <see cref="IndexBuildStateTests"/> proves <c>MarkNotReady</c> does the right thing when called.
/// This proves it is actually called, and called <b>before</b> the indexes are dropped — a test of the
/// state alone would pass happily while the builder never invoked it, which is precisely how the
/// defect survived review the first time.
/// </para>
/// <para>
/// Order is the assertion, not just the call. Marking not-ready after the drop leaves a window in
/// which the documents are gone and the readiness probe still reports 200.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexBuilderHostedService))]
public class IndexBuilderRecreateOrderTests
{
    private IIndexBuildState _buildState = null!;
    private ICatalogIndexService _catalogIndex = null!;
    private ICodeListEntryIndexService _codeListIndex = null!;
    private IndexBuilderHostedService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _buildState = Substitute.For<IIndexBuildState>();
        _catalogIndex = Substitute.For<ICatalogIndexService>();
        _codeListIndex = Substitute.For<ICodeListEntryIndexService>();

        var builder = Substitute.For<IIndexBuilderService>();
        builder.BuildIndexAsync(Arg.Any<CancellationToken>())
            .Returns(new IndexBuildReport(StructuresAvailable: true));

        var provider = new ServiceCollection()
            .AddScoped(_ => _catalogIndex)
            .AddScoped(_ => _codeListIndex)
            .AddScoped(_ => builder)
            .BuildServiceProvider();

        _sut = new IndexBuilderHostedService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            _buildState,
            Substitute.For<IIndexRebuildTrigger>(),
            Options.Create(new IndexSearchOptions()),
            Options.Create(new ElasticsearchOptions()),
            NullLogger<IndexBuilderHostedService>.Instance);
    }

    [Test]
    public async Task Recreating_withdraws_readiness_before_dropping_the_indexes()
    {
        await _sut.BuildBothIndexesAsync(recreate: true, CancellationToken.None);

        Received.InOrder(() =>
        {
            _buildState.MarkNotReady();
            _catalogIndex.EnsureIndexAsync(true, Arg.Any<CancellationToken>());
            _codeListIndex.EnsureIndexAsync(true, Arg.Any<CancellationToken>());
        });
    }

    /// <summary>
    /// The other half: a plain rebuild writes over documents that stay searchable throughout, so
    /// withdrawing readiness there would take a healthy replica out of rotation for no reason.
    /// </summary>
    [Test]
    public async Task A_plain_rebuild_neither_drops_the_indexes_nor_withdraws_readiness()
    {
        await _sut.BuildBothIndexesAsync(recreate: false, CancellationToken.None);

        _buildState.DidNotReceive().MarkNotReady();
        await _catalogIndex.DidNotReceive().EnsureIndexAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
        await _codeListIndex.DidNotReceive().EnsureIndexAsync(Arg.Any<bool>(), Arg.Any<CancellationToken>());
    }

    [Test]
    public async Task The_structures_verdict_is_returned_to_the_caller()
    {
        var result = await _sut.BuildBothIndexesAsync(recreate: false, CancellationToken.None);

        result.Should().BeTrue(because: "GET /api/Index reports it, and it is how an empty Structures facet is diagnosed");
    }
}
