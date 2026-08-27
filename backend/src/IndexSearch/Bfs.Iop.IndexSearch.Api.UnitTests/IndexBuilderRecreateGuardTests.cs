using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.UnitTests;

/// <summary>
/// Guards the one combination that empties the index permanently.
/// <para>
/// Recreating drops both indexes and recreates them <b>empty</b>; the build that follows is what
/// refills them. With <c>BuildIndexOnStartup</c> off, nothing does — and unlike every other failure
/// in this service, it does not heal on the next restart, because the next restart drops the index
/// again. Searches return HTTP 200 with no results until a human notices.
/// </para>
/// <para>
/// The combination was unreachable in practice while <c>RecreateIndexOnStartup</c> defaulted to
/// false. It became reachable the moment that default flipped to true, so one setting now has to
/// know about the other.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexBuilderHostedService))]
public class IndexBuilderRecreateGuardTests
{
    private static bool ShouldRecreate(bool recreateOnStartup, bool buildOnStartup) =>
        IndexBuilderHostedService.ShouldRecreate(
            recreateOnStartup, buildOnStartup, NullLogger.Instance);

    [Test]
    public void Recreates_when_a_build_follows()
    {
        ShouldRecreate(recreateOnStartup: true, buildOnStartup: true)
            .Should().BeTrue(because: "this is the configured default: a fresh index every restart");
    }

    [Test]
    public void Does_not_recreate_when_no_build_would_follow()
    {
        ShouldRecreate(recreateOnStartup: true, buildOnStartup: false)
            .Should().BeFalse(
                because: "dropping the indexes with nothing to refill them serves zero results "
                       + "indefinitely; a stale but populated index is the safer of the two");
    }

    [Test]
    public void Never_recreates_when_the_setting_is_off()
    {
        ShouldRecreate(recreateOnStartup: false, buildOnStartup: true).Should().BeFalse();
        ShouldRecreate(recreateOnStartup: false, buildOnStartup: false).Should().BeFalse();
    }
}
