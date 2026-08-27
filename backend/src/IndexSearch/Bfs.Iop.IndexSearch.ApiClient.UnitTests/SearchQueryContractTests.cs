using System.Reflection;
using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.IndexSearch.Api.Controllers;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

/// <summary>
/// Asserts the client's query-string names against the controller's actual parameter names, read by
/// reflection.
/// <para>
/// The sibling <c>IndexSearchSearchClientQueryTests</c> pins the names as literals, which catches a
/// client-side typo but would still pass if the client and the test were wrong in the same way. This
/// fixture removes that blind spot: the expected set comes from
/// <see cref="SearchController"/> itself, so drift on **either** side fails the build — including a
/// controller parameter being renamed, which no client-only test can see.
/// </para>
/// </summary>
[TestFixture(TestOf = typeof(IndexSearchSearchClient))]
public class SearchQueryContractTests
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

    private static IReadOnlyCollection<string> ControllerParameterNames(string methodName) =>
        [.. typeof(SearchController)
            .GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance)!
            .GetParameters()
            .Select(x => x.Name!)
            // Not bound from the query string.
            .Where(x => x != "cancellationToken")];

    /// <param name="call">The client call to capture.</param>
    /// <param name="responseJson">
    /// Must match what the call deserializes into — "[]" for the list endpoints, "{}" for the count
    /// endpoint. A mismatch throws during deserialization rather than failing the assertion, which is
    /// noisy but at least never silently passes.
    /// </param>
    private static async Task<IReadOnlyList<string>> EmittedNamesAsync(
        Func<IndexSearchSearchClient, Task> call,
        string responseJson = "[]")
    {
        var handler = new CapturingHttpMessageHandler(responseJson);

        using var httpClient = new HttpClient(handler) { BaseAddress = new Uri("http://indexsearch.test/") };

        await call(new IndexSearchSearchClient(httpClient));

        return [.. handler.QueryParameters.Select(x => x.Key).Distinct()];
    }

    [Test]
    public async Task Every_name_the_search_client_emits_is_a_parameter_the_controller_binds()
    {
        var emitted = await EmittedNamesAsync(client =>
            client.SearchAsync("bev", "de", FullFilter, page: 2, pageSize: 25));

        emitted.Should().BeSubsetOf(ControllerParameterNames(nameof(SearchController.Search)));
    }

    [Test]
    public async Task The_search_client_exercises_every_filter_the_controller_accepts()
    {
        var emitted = await EmittedNamesAsync(client =>
            client.SearchAsync("bev", "de", FullFilter, page: 2, pageSize: 25));

        // The other direction: a filter the controller accepts but the client never sends would be a
        // silently unsupported filter. With every filter populated above, the two sets must match.
        ControllerParameterNames(nameof(SearchController.Search))
            .Should().BeSubsetOf(emitted);
    }

    [Test]
    public async Task Every_name_the_count_client_emits_is_a_parameter_the_count_action_binds()
    {
        var emitted = await EmittedNamesAsync(client =>
            client.SearchCountAsync("bev", "de", FullFilter), responseJson: "{}");

        emitted.Should().BeSubsetOf(ControllerParameterNames(nameof(SearchController.SearchCount)));
    }

    [Test]
    public async Task The_count_client_exercises_every_filter_the_count_action_accepts()
    {
        var emitted = await EmittedNamesAsync(client =>
            client.SearchCountAsync("bev", "de", FullFilter), responseJson: "{}");

        // Count takes no paging, so exclude those two from the expected set.
        var expected = ControllerParameterNames(nameof(SearchController.SearchCount))
            .Where(x => x is not ("page" or "pageSize"));

        expected.Should().BeSubsetOf(emitted);
    }

    [Test]
    public async Task Every_name_the_codelist_client_emits_is_a_parameter_the_concepts_action_binds()
    {
        var emitted = await EmittedNamesAsync(client =>
            client.SearchCodeListEntriesAsync(
                Guid.NewGuid(),
                "de",
                "abc",
                ["f1"],
                addCodeListEntriesPaths: true,
                page: 1,
                pageSize: 50));

        var bound = typeof(ConceptsController)
            .GetMethod(nameof(ConceptsController.SearchCodeListEntries), BindingFlags.Public | BindingFlags.Instance)!
            .GetParameters()
            .Select(x => x.Name!)
            // "id" comes from the route, not the query string.
            .Where(x => x is not ("cancellationToken" or "id"))
            .ToArray();

        emitted.Should().BeSubsetOf(bound);
        bound.Should().BeSubsetOf(emitted);
    }
}
