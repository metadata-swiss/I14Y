using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Commands.Catalog;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.Core.CommandHandlers.Catalog;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.ApiClient;
using Bfs.Iop.Infrastructure.ApiClient;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

using CatalogSearchHit = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchHit;
using CatalogSearchRequest = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchRequest;
using CatalogSearchSort = Bfs.Iop.IndexSearch.Contracts.Search.CatalogSearchSort;

namespace Bfs.Iop.Core.UnitTests.Commands.Catalog;

[TestFixture(TestOf = typeof(GetCatalogSearchCommandHandler))]
internal sealed class GetCatalogSearchCommandHandlerTests
{
    private IIndexSearchApiClient _search = null!;
    private CatalogSearchRequest? _sent;

    [SetUp]
    public void SetUp()
    {
        _sent = null;
        _search = Substitute.For<IIndexSearchApiClient>();

        _search.PostSearchCatalogByBodyAsync(Arg.Do<CatalogSearchRequest>(x => _sent = x), Arg.Any<CancellationToken>())
            .Returns(new SwaggerResponse<CatalogSearchHitPagedResult>(
                200, new Dictionary<string, IEnumerable<string>>(), new CatalogSearchHitPagedResult { Results = [] }));

        _search.PostSearchCatalogAllByBodyAsync(Arg.Do<CatalogSearchRequest>(x => _sent = x), Arg.Any<CancellationToken>())
            .Returns(new SwaggerResponse<ICollection<CatalogSearchHit>>(
                200, new Dictionary<string, IEnumerable<string>>(), []));
    }

    [TestCase(CatalogSearchSort.Relevance)]
    [TestCase(CatalogSearchSort.TitleAscending)]
    [TestCase(CatalogSearchSort.TitleDescending)]
    [TestCase(CatalogSearchSort.ModifiedNewest)]
    [TestCase(CatalogSearchSort.ModifiedOldest)]
    public async Task Given_a_sort_When_searching_one_page_Then_the_index_is_asked_to_sort_that_way(CatalogSearchSort sort)
    {
        await WhenSearching(sort, page: 1, pageSize: 10);

        _sent!.Sort.Should().Be(sort);
    }

    [Test]
    public async Task Given_a_sort_When_searching_every_page_Then_the_index_is_asked_to_sort_that_way()
    {
        await WhenSearching(CatalogSearchSort.ModifiedOldest, page: null, pageSize: null);

        _sent!.Sort.Should().Be(CatalogSearchSort.ModifiedOldest);
    }

    private Task WhenSearching(CatalogSearchSort sort, int? page, int? pageSize) =>
        new GetCatalogSearchCommandHandler(
                _search,
                Substitute.For<IVocabulariesService>(),
                Substitute.For<IAgentsService>(),
                NullLogger<GetCatalogSearchCommandHandler>.Instance)
            .Handle(new GetCatalogSearchCommand(null, "fr", new CatalogSearchFilter(), sort, page, pageSize), CancellationToken.None);
}
