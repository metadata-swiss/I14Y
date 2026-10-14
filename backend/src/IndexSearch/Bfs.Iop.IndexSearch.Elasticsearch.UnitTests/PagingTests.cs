using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(Paging))]
public class PagingTests
{
    private const int Window = Paging.MaxResultWindow;

    [Test]
    public void An_ordinary_page_is_translated_straight_through()
    {
        Paging.ToWindow(page: 3, pageSize: 20).Should().Be((40, 20));
    }

    [Test]
    public void The_first_page_starts_at_the_beginning()
    {
        Paging.ToWindow(page: 1, pageSize: 20).Should().Be((0, 20));
    }

    [TestCase(0)]
    [TestCase(-5)]
    public void A_page_below_one_is_treated_as_the_first(int page)
    {
        Paging.ToWindow(page, pageSize: 20).Should().Be((0, 20));
    }

    [Test]
    public void A_page_past_the_result_window_returns_nothing_rather_than_a_different_page()
    {
        // Two pages beyond the last one the index will serve. Clamping `from` would have answered with
        // the last page's rows under the requested page number — wrong data, silently. An empty page is
        // the truthful answer, and the total count still tells the caller where the end is.
        Paging.ToWindow(page: (Window / 20) + 2, pageSize: 20).Should().Be((0, 0));
    }

    [Test]
    public void The_last_servable_page_is_still_served()
    {
        // Its last row is the last the window holds, so `from` plus `size` lands exactly on it.
        Paging.ToWindow(page: Window / 20, pageSize: 20).Should().Be((Window - 20, 20));
    }

    [Test]
    public void A_page_straddling_the_window_is_trimmed_to_what_the_index_will_serve()
    {
        // Starting 100 short of the window, so only 100 rows fit however many were asked for.
        Paging.ToWindow(page: ((Window - 100) / 100) + 1, pageSize: 100).Should().Be((Window - 100, 100));
        Paging.ToWindow(page: ((Window - 100) / 300) + 1, pageSize: 300).Should().Be((Window - 100, 100));
    }

    // Core asks for everything with int.MaxValue, and (page - 1) * pageSize overflows int long before
    // it reaches the window, which is why the arithmetic is done in long.
    [Test]
    public void No_window_ever_exceeds_what_elasticsearch_will_serve()
    {
        foreach (var page in new[] { 1, 2, 50, (Window / 20) - 1, Window / 20, (Window / 20) + 1, int.MaxValue })
        {
            foreach (var pageSize in new[] { 1, 20, 200, Window, int.MaxValue })
            {
                var (from, size) = Paging.ToWindow(page, pageSize);

                (from + (long)size).Should().BeLessThanOrEqualTo(
                    Paging.MaxResultWindow,
                    "page {0} of {1} would otherwise be rejected by Elasticsearch",
                    page,
                    pageSize);
            }
        }
    }
}
