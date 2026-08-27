using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Controllers;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// The two rebuild routes differ only in whether they purge, and both answer 202, so nothing about a
/// response tells you which one you called. These tests are what keeps them distinguishable.
/// </summary>
[TestFixture(TestOf = typeof(IndexController))]
public class IndexRebuildEndpointTests
{
    private IIndexRebuildTrigger _trigger = null!;
    private IIndexBuildState _buildState = null!;
    private IndexController _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _trigger = Substitute.For<IIndexRebuildTrigger>();
        _buildState = Substitute.For<IIndexBuildState>();

        _sut = new IndexController(
            Substitute.For<IIndexEventQueue>(),
            _buildState,
            _trigger,
            NullLogger<IndexController>.Instance);
    }

    [Test]
    public void Rebuild_requests_a_build_that_does_not_purge()
    {
        _sut.Rebuild().Should().BeOfType<AcceptedResult>();

        _trigger.Received(1).Request(recreate: false);
    }

    [Test]
    public void Recreate_requests_a_build_that_purges()
    {
        _sut.Recreate().Should().BeOfType<AcceptedResult>();

        _trigger.Received(1).Request(recreate: true);
    }

    /// <summary>
    /// Two concurrent full builds would write over each other's documents. Refusing is also what
    /// makes the endpoint safe to retry: a caller polling GET /api/Index cannot stack up rebuilds.
    /// </summary>
    [Test]
    public void Neither_route_starts_a_second_build_while_one_is_running()
    {
        _buildState.IsFullBuildRunning.Returns(true);

        _sut.Rebuild().Should().BeOfType<ConflictObjectResult>();
        _sut.Recreate().Should().BeOfType<ConflictObjectResult>();

        _trigger.DidNotReceive().Request(Arg.Any<bool>());
    }
}
