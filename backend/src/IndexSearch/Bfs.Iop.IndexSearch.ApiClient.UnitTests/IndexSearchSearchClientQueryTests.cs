using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

/// <summary>
/// The query-parameter names this client emits must match the <c>[FromQuery]</c> parameter names on
/// the IndexSearch and IOP Core search controllers exactly.
/// <para>
/// This is the highest-value test in the client: the names are hand-written, and a typo does not
/// fail — the server silently ignores the unknown parameter and returns unfiltered results. That
/// looks like a search relevance problem, not a wiring bug, so it can survive review and testing.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexSearchSearchClient))]
public class IndexSearchSearchClientQueryTests
{
    private static readonly CatalogSearchFilter FullFilter = new()
    {
        AccessRights = ["NON_PUBLIC"],
        BusinessEvents = ["be-1"],
        ConceptValueTypes = [ConceptType.CodeList],
        Formats = ["CSV"],
        LifeEvents = ["le-1"],
        PublicationLevels = [PublicationLevel.Public],
        PublicationLevelProposals = [PublicationLevel.Internal],
        PublisherIdentifiers = ["pub-1"],
        RegistrationStatuses = [RegistrationStatus.Recorded],
        RegistrationStatusProposals = [RegistrationStatus.Qualified],
        Structure = SearchStructureOption.WithStructure,
        Themes = ["theme-1"],
        Types = [SearchResourceType.Dataset],
    };

    private static (IndexSearchSearchClient Client, CapturingHttpMessageHandler Handler) CreateClient(
        string responseJson = "[]",
        IReadOnlyDictionary<string, string>? responseHeaders = null)
    {
        var handler = new CapturingHttpMessageHandler(responseJson, responseHeaders: responseHeaders);

        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("http://indexsearch.test/"),
        };

        return (new IndexSearchSearchClient(httpClient), handler);
    }

    [Test]
    public async Task SearchAsync_emits_every_filter_under_the_name_the_controllers_bind()
    {
        var (client, handler) = CreateClient();

        await client.SearchAsync("bev", "de", FullFilter, page: 2, pageSize: 25);

        handler.Request!.RequestUri!.AbsolutePath.Should().Be("/api/Search");

        // Names, not just presence: each of these is the exact [FromQuery] parameter name.
        handler.ValuesOf("query").Should().Equal("bev");
        handler.ValuesOf("language").Should().Equal("de");
        handler.ValuesOf("accessRights").Should().Equal("NON_PUBLIC");
        handler.ValuesOf("businessEvents").Should().Equal("be-1");
        handler.ValuesOf("conceptValueTypes").Should().Equal("CodeList");
        handler.ValuesOf("formats").Should().Equal("CSV");
        handler.ValuesOf("levels").Should().Equal("Public");
        handler.ValuesOf("levelProposals").Should().Equal("Internal");
        handler.ValuesOf("lifeEvents").Should().Equal("le-1");
        handler.ValuesOf("publishers").Should().Equal("pub-1");
        handler.ValuesOf("statuses").Should().Equal("Recorded");
        handler.ValuesOf("statusProposals").Should().Equal("Qualified");
        handler.ValuesOf("structure").Should().Equal("WithStructure");
        handler.ValuesOf("themes").Should().Equal("theme-1");
        handler.ValuesOf("types").Should().Equal("Dataset");
        handler.ValuesOf("page").Should().Equal("2");
        handler.ValuesOf("pageSize").Should().Equal("25");
    }

    [Test]
    public async Task SearchAsync_repeats_the_key_for_multi_valued_filters()
    {
        var (client, handler) = CreateClient();

        var filter = new CatalogSearchFilter
        {
            Types = [SearchResourceType.Dataset, SearchResourceType.Concept],
        };

        await client.SearchAsync(query: null, language: null, filter, page: null, pageSize: null);

        // "types=Dataset&types=Concept", never "types=Dataset,Concept": ASP.NET binds a string[] from
        // repeated keys, and a comma-joined value arrives as ONE element containing a comma.
        handler.ValuesOf("types").Should().Equal("Dataset", "Concept");
    }

    [Test]
    public async Task SearchAsync_omits_absent_values_rather_than_sending_them_empty()
    {
        var (client, handler) = CreateClient();

        await client.SearchAsync(query: null, language: null, filter: null, page: null, pageSize: null);

        // An empty "query=" is not the same as no query to a controller that checks for whitespace,
        // and an empty "page=" fails int binding outright.
        handler.QueryParameters.Should().BeEmpty();
        handler.Request!.RequestUri!.AbsolutePath.Should().Be("/api/Search");
    }

    [Test]
    public async Task SearchAsync_reads_paging_from_the_response_headers()
    {
        var headers = new Dictionary<string, string>
        {
            ["x-paging-page"] = "3",
            ["x-paging-pagesize"] = "25",
            ["x-paging-totalrows"] = "417",
        };

        var (client, _) = CreateClient("[]", headers);

        var result = await client.SearchAsync("bev", "de", filter: null, page: 3, pageSize: 25);

        result.Page.Should().Be(3);
        result.PageSize.Should().Be(25);
        result.TotalCount.Should().Be(417);
    }

    /// <summary>
    /// The service omits the totals headers when there are no hits; the client must not throw — and
    /// must not report page 0 either.
    /// <para>
    /// This test previously asserted <c>Page == 0</c>, which encoded a defect rather than a decision:
    /// IOP Core feeds this into <c>AddPagingHeaders</c>, which rejects a non-positive page, so a
    /// successful empty search surfaced as an HTTP 400. Paging fallbacks are covered in depth by
    /// <c>IndexSearchSearchClientPagingTests</c>; this case is kept here because "no hits" is the
    /// realistic way to reach it.
    /// </para>
    /// </summary>
    [Test]
    public async Task SearchAsync_falls_back_to_page_one_when_the_headers_are_absent()
    {
        var (client, _) = CreateClient();

        var result = await client.SearchAsync("bev", "de", filter: null, page: null, pageSize: null);

        result.Page.Should().Be(1);
        result.TotalCount.Should().Be(0);
        result.Results.Should().BeEmpty();
    }

    [Test]
    public async Task SearchCountAsync_hits_the_count_route_with_the_same_filter_names()
    {
        var (client, handler) = CreateClient("{}");

        await client.SearchCountAsync("bev", "de", FullFilter);

        handler.Request!.RequestUri!.AbsolutePath.Should().Be("/api/Search/count");
        handler.ValuesOf("types").Should().Equal("Dataset");
        handler.ValuesOf("structure").Should().Equal("WithStructure");

        // Count takes no paging; sending it would be silently ignored, so assert it is absent.
        handler.ValuesOf("page").Should().BeEmpty();
        handler.ValuesOf("pageSize").Should().BeEmpty();
    }

    [Test]
    public async Task SearchCodeListEntriesAsync_uses_the_concept_scoped_route()
    {
        var conceptId = Guid.Parse("11111111-2222-3333-4444-555555555555");
        var (client, handler) = CreateClient();

        await client.SearchCodeListEntriesAsync(
            conceptId,
            "de",
            "abc",
            ["f1", "f2"],
            addCodeListEntriesPaths: true,
            page: 1,
            pageSize: 50);

        handler.Request!.RequestUri!.AbsolutePath
            .Should().Be($"/api/Concepts/{conceptId}/codelist-entries/search");

        handler.ValuesOf("language").Should().Equal("de");
        handler.ValuesOf("query").Should().Equal("abc");
        handler.ValuesOf("filters").Should().Equal("f1", "f2");
        handler.ValuesOf("addCodeListEntriesPaths").Should().Equal("true");
    }

    [Test]
    public async Task SearchCodeListEntriesAsync_sends_addCodeListEntriesPaths_false_explicitly()
    {
        var (client, handler) = CreateClient();

        await client.SearchCodeListEntriesAsync(
            Guid.NewGuid(),
            "de",
            query: null,
            filters: null,
            addCodeListEntriesPaths: false,
            page: null,
            pageSize: null);

        // Must be sent, not omitted: omitting a bool would let the server's default win, which is a
        // different answer from the one the caller asked for.
        handler.ValuesOf("addCodeListEntriesPaths").Should().Equal("false");
    }
}
