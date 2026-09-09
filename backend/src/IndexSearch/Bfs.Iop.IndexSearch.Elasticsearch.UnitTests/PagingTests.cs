using AwesomeAssertions;
using Bfs.Iop.IndexSearch.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Elasticsearch.UnitTests;

[TestFixture(TestOf = typeof(Paging))]
public class PagingTests
{
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
        // Page 1000 of 20 starts at 19,980, past the 10,000 the index will serve. Clamping `from`
        // would have answered with page 501's rows labelled page 1000 — wrong data, silently. An empty
        // page is the truthful answer, and the total count still tells the caller where the end is.
        Paging.ToWindow(page: 1000, pageSize: 20).Should().Be((0, 0));
    }

    [Test]
    public void The_last_servable_page_is_still_served()
    {
        // from 9,980 + size 20 lands exactly on the window.
        Paging.ToWindow(page: 500, pageSize: 20).Should().Be((9_980, 20));
    }

    [Test]
    public void A_page_straddling_the_window_is_trimmed_to_what_the_index_will_serve()
    {
        // from 9,900, so only 100 of the 200 asked for fit.
        Paging.ToWindow(page: 100, pageSize: 100).Should().Be((9_900, 100));
        Paging.ToWindow(page: 34, pageSize: 300).Should().Be((9_900, 100));
    }

    [Test]
    public void An_everything_page_size_does_not_overflow()
    {
        // Core asks for everything with int.MaxValue. (page - 1) * pageSize overflows int long before
        // it reaches the window, which is why the arithmetic is done in long.
        var (from, size) = Paging.ToWindow(page: 2, pageSize: int.MaxValue);

        from.Should().BeGreaterThanOrEqualTo(0);
        size.Should().BeGreaterThanOrEqualTo(0);
        (from + size).Should().BeLessThanOrEqualTo(Paging.MaxResultWindow);
    }

    [Test]
    public void No_window_ever_exceeds_what_elasticsearch_will_serve()
    {
        foreach (var page in new[] { 1, 2, 50, 499, 500, 501, 1000, int.MaxValue })
        {
            foreach (var pageSize in new[] { 1, 20, 200, 10_000, int.MaxValue })
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
