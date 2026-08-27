using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.CommandHandlers.Catalog;
using Bfs.Iop.IndexSearch.ApiClient;
using NSubstitute;

namespace Bfs.Iop.Core.UnitTests.CommandHandlers.Catalog;

/// <summary>
/// The handler forwards to the search port, so the only behaviour it owns is the paging default —
/// and that default is load-bearing.
/// <para>
/// "No paging supplied" means "return everything", expressed as page 1 with an unbounded size. Get it
/// wrong in one direction and callers who omit paging silently receive only the first page of a
/// catalogue; wrong in the other and a caller asking for page 3 gets page 1. Neither throws, and both
/// look like a data problem rather than a paging one.
/// </para>
/// <para>
/// It is also an <b>either/or</b>, not two independent defaults: supplying only one of the two is
/// treated as supplying neither, because a page number without a page size does not describe a page.
/// </para>
/// </summary>
[TestFixture]
public sealed class GetCatalogSearchCommandHandlerTests
{
    private IIndexSearchSearchClient _searchClient = null!;
    private GetCatalogSearchCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _searchClient = Substitute.For<IIndexSearchSearchClient>();
        _searchClient
            .SearchAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CatalogSearchFilter?>(),
                Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(new PagedResult<SearchResultModel>());

        _sut = new GetCatalogSearchCommandHandler(_searchClient);
    }

    private Task Handle(int? page, int? pageSize) =>
        _sut.Handle(
            new GetCatalogSearchCommand("bev", "de", new CatalogSearchFilter(), page, pageSize),
            CancellationToken.None);

    private Task Received(int page, int pageSize) =>
        _searchClient.Received(1).SearchAsync(
            Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CatalogSearchFilter?>(),
            page, pageSize, Arg.Any<CancellationToken>());

    [Test]
    public async Task BothSupplied_ArePassedThroughUnchanged()
    {
        await Handle(page: 3, pageSize: 25);

        await Received(3, 25);
    }

    [Test]
    public async Task NeitherSupplied_MeansEverything()
    {
        await Handle(page: null, pageSize: null);

        await Received(1, int.MaxValue);
    }

    /// <summary>A page number without a size does not describe a page, so it is treated as neither.</summary>
    [Test]
    public async Task OnlyPageSupplied_FallsBackToEverything()
    {
        await Handle(page: 3, pageSize: null);

        await Received(1, int.MaxValue);
    }

    [Test]
    public async Task OnlyPageSizeSupplied_FallsBackToEverything()
    {
        await Handle(page: null, pageSize: 25);

        await Received(1, int.MaxValue);
    }

    /// <summary>
    /// The handler must not reshape the result. The service behind the port already returns finished
    /// models — re-mapping here would be a second copy of that logic, free to drift from the one on
    /// the other side of the wire.
    /// </summary>
    [Test]
    public async Task TheServiceResultIsReturnedUntouched()
    {
        var expected = new PagedResult<SearchResultModel> { Page = 7, PageSize = 11, TotalCount = 42 };

        _searchClient
            .SearchAsync(
                Arg.Any<string?>(), Arg.Any<string?>(), Arg.Any<CatalogSearchFilter?>(),
                Arg.Any<int?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
            .Returns(expected);

        var result = await _sut.Handle(
            new GetCatalogSearchCommand(null, null, new CatalogSearchFilter(), 7, 11),
            CancellationToken.None);

        result.Should().BeSameAs(expected);
    }
}
