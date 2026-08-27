using AwesomeAssertions;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

/// <summary>
/// Paging comes back in response headers, not the body. What happens when a header is missing is not
/// a cosmetic detail.
/// <para>
/// Callers put this straight into <c>PagedResult</c>, and IOP Core then hands that to
/// <c>AddPagingHeaders</c>, which rejects a non-positive page. So returning 0 for an absent header
/// turns a perfectly successful search into an HTTP 400 — an error the user cannot act on, about a
/// request that actually worked.
/// </para>
/// <para>
/// This path did not exist before the gateway migration: paging used to come from an in-process
/// in-process result that always set it. It became reachable the moment paging started crossing a
/// network boundary, which is why it is pinned here.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexSearchSearchClient))]
public class IndexSearchSearchClientPagingTests
{
    private static IndexSearchSearchClient ClientReturning(
        string body,
        Dictionary<string, string>? headers = null)
    {
        var handler = new CapturingHttpMessageHandler(body, responseHeaders: headers);

        return new IndexSearchSearchClient(
            new HttpClient(handler) { BaseAddress = new Uri("http://indexsearch.test/") });
    }

    [Test]
    public async Task WhenTheServiceReportsPaging_ItIsUsed()
    {
        var client = ClientReturning("[]", new Dictionary<string, string>
        {
            ["x-paging-page"] = "3",
            ["x-paging-pagesize"] = "25",
            ["x-paging-totalrows"] = "70",
        });

        var result = await client.SearchAsync(null, null, null, page: 3, pageSize: 25);

        result.Page.Should().Be(3);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().Be(70);
    }

    /// <summary>
    /// The regression this guards: a missing header must NOT yield Page = 0. Echoing the requested
    /// page is the honest answer — we asked for page 2, and nothing said otherwise.
    /// </summary>
    [Test]
    public async Task WhenPagingHeadersAreMissing_TheRequestedPageIsEchoed()
    {
        var client = ClientReturning("[]");

        var result = await client.SearchAsync(null, null, null, page: 2, pageSize: 50);

        result.Page.Should().Be(2);
        result.PageSize.Should().Be(50);
    }

    /// <summary>
    /// No paging requested and none reported means "everything", so page 1 is the only sane answer —
    /// never 0.
    /// </summary>
    [Test]
    public async Task WhenNothingIsRequestedOrReported_PageIsOneNotZero()
    {
        var client = ClientReturning("[]");

        var result = await client.SearchAsync(null, null, null, page: null, pageSize: null);

        result.Page.Should().Be(1);
        result.Page.Should().BePositive("AddPagingHeaders rejects a non-positive page");
    }

    /// <summary>
    /// Totals are different: there is no request value to echo, and 0 is a truthful "none reported" —
    /// the same convention Core uses for an empty result set.
    /// </summary>
    [Test]
    public async Task AnAbsentTotalStaysZero()
    {
        var client = ClientReturning("[]");

        var result = await client.SearchAsync(null, null, null, page: 1, pageSize: 10);

        result.TotalCount.Should().Be(0);
    }

    [Test]
    public async Task CodeListSearch_AppliesTheSameFallback()
    {
        var client = ClientReturning("[]");

        var result = await client.SearchCodeListEntriesAsync(
            Guid.NewGuid(), "de", null, null, addCodeListEntriesPaths: false, page: 4, pageSize: 20);

        result.Page.Should().Be(4);
        result.PageSize.Should().Be(20);
    }
}
