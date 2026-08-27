using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Abstractions.Models.Search.Filters;

namespace Bfs.Iop.IndexSearch.ApiClient;

/// <inheritdoc cref="IIndexSearchSearchClient"/>
internal sealed class IndexSearchSearchClient : IIndexSearchSearchClient
{
    /// <summary>Paging header names, matching Bfs.Iop.Core.Common's HttpContextExtensions.</summary>
    private const string PageHeader = "x-paging-page";
    private const string PageSizeHeader = "x-paging-pagesize";
    private const string TotalRowsHeader = "x-paging-totalrows";

    private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() },
    };

    private readonly HttpClient _httpClient;

    public IndexSearchSearchClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<PagedResult<SearchResultModel>> SearchAsync(
        string? query,
        string? language,
        CatalogSearchFilter? filter,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var parameters = CatalogParameters(query, language, filter);

        Add(parameters, "page", page?.ToString(CultureInfo.InvariantCulture));
        Add(parameters, "pageSize", pageSize?.ToString(CultureInfo.InvariantCulture));

        using var response = await _httpClient.GetAsync(BuildUrl("api/Search", parameters), cancellationToken);

        response.EnsureSuccessStatusCode();

        var results = await response.Content.ReadFromJsonAsync<List<SearchResultModel>>(_jsonOptions, cancellationToken) ?? [];

        // The service returns paging in headers, not in the body. Reading them here keeps callers
        // from having to know that protocol — and keeps the shape identical to Core's PagedResult.
        return new PagedResult<SearchResultModel>
        {
            Page = HeaderInt(response, PageHeader, page ?? 1),
            PageSize = HeaderInt(response, PageSizeHeader, pageSize ?? results.Count),
            TotalCount = HeaderInt(response, TotalRowsHeader),
            Results = results,
        };
    }

    public async Task<SearchCountResultModel> SearchCountAsync(
        string? query,
        string? language,
        CatalogSearchFilter? filter,
        CancellationToken cancellationToken = default)
    {
        var parameters = CatalogParameters(query, language, filter);

        using var response = await _httpClient.GetAsync(BuildUrl("api/Search/count", parameters), cancellationToken);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<SearchCountResultModel>(_jsonOptions, cancellationToken)
            ?? new SearchCountResultModel();
    }

    public async Task<PagedResult<CodeListEntrySearchResultEntryModel>> SearchCodeListEntriesAsync(
        Guid conceptId,
        string language,
        string? query,
        IEnumerable<string>? filters,
        bool addCodeListEntriesPaths,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken = default)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        Add(parameters, "language", language);
        Add(parameters, "query", query);
        AddMany(parameters, "filters", filters);
        Add(parameters, "addCodeListEntriesPaths", addCodeListEntriesPaths ? "true" : "false");
        Add(parameters, "page", page?.ToString(CultureInfo.InvariantCulture));
        Add(parameters, "pageSize", pageSize?.ToString(CultureInfo.InvariantCulture));

        var route = $"api/Concepts/{conceptId}/codelist-entries/search";

        using var response = await _httpClient.GetAsync(BuildUrl(route, parameters), cancellationToken);

        response.EnsureSuccessStatusCode();

        var results = await response.Content
            .ReadFromJsonAsync<List<CodeListEntrySearchResultEntryModel>>(_jsonOptions, cancellationToken) ?? [];

        return new PagedResult<CodeListEntrySearchResultEntryModel>
        {
            Page = HeaderInt(response, PageHeader, page ?? 1),
            PageSize = HeaderInt(response, PageSizeHeader, pageSize ?? results.Count),
            TotalCount = HeaderInt(response, TotalRowsHeader),
            Results = results,
        };
    }

    /// <summary>
    /// The single place every catalog query parameter is named.
    /// <para>
    /// These names must match <c>SearchController</c>'s <c>[FromQuery]</c> parameters on both hosts
    /// exactly — a typo here does not fail, it silently drops a filter, which looks like a search
    /// relevance problem rather than a wiring bug. Keeping them in one method means a contract change
    /// shows up as a single reviewable diff.
    /// </para>
    /// </summary>
    private static List<KeyValuePair<string, string>> CatalogParameters(
        string? query,
        string? language,
        CatalogSearchFilter? filter)
    {
        var parameters = new List<KeyValuePair<string, string>>();

        Add(parameters, "language", language);
        Add(parameters, "query", query);

        if (filter is null)
        {
            return parameters;
        }

        AddMany(parameters, "accessRights", filter.AccessRights);
        AddMany(parameters, "businessEvents", filter.BusinessEvents);
        AddMany(parameters, "conceptValueTypes", filter.ConceptValueTypes.Select(x => x.ToString()));
        AddMany(parameters, "formats", filter.Formats);
        AddMany(parameters, "levels", filter.PublicationLevels.Select(x => x.ToString()));
        AddMany(parameters, "levelProposals", filter.PublicationLevelProposals.Select(x => x.ToString()));
        AddMany(parameters, "lifeEvents", filter.LifeEvents);
        AddMany(parameters, "publishers", filter.PublisherIdentifiers);
        AddMany(parameters, "statuses", filter.RegistrationStatuses.Select(x => x.ToString()));
        AddMany(parameters, "statusProposals", filter.RegistrationStatusProposals.Select(x => x.ToString()));
        Add(parameters, "structure", filter.Structure?.ToString());
        AddMany(parameters, "themes", filter.Themes);
        AddMany(parameters, "types", filter.Types.Select(x => x.ToString()));

        return parameters;
    }

    private static void Add(List<KeyValuePair<string, string>> parameters, string name, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            parameters.Add(new(name, value));
        }
    }

    private static void AddMany(List<KeyValuePair<string, string>> parameters, string name, IEnumerable<string>? values)
    {
        foreach (var value in values ?? [])
        {
            Add(parameters, name, value);
        }
    }

    private static string BuildUrl(string route, List<KeyValuePair<string, string>> parameters)
    {
        if (parameters.Count == 0)
        {
            return route;
        }

        // Repeated keys, not comma-joined: ASP.NET binds string[] from "types=A&types=B", and a
        // comma-joined value would arrive as one array element containing a comma.
        var queryString = string.Join(
            '&',
            parameters.Select(x => $"{Uri.EscapeDataString(x.Key)}={Uri.EscapeDataString(x.Value)}"));

        return $"{route}?{queryString}";
    }

    /// <summary>
    /// Reads a paging header, falling back to <paramref name="fallback"/> when it is absent or
    /// unparseable.
    /// <para>
    /// The fallback matters: callers put the result straight into <c>PagedResult</c>, and a
    /// <c>PagedResult</c> with <c>Page = 0</c> is not a valid answer to "which page is this?" — it is
    /// a missing answer wearing a plausible number. Core then hands it to <c>AddPagingHeaders</c>,
    /// which rejects a non-positive page, so a successful search would surface as a 400.
    /// </para>
    /// <para>
    /// Echoing what we asked for is the honest fallback: if the service did not say which page it
    /// returned, the page we requested is the best available answer. Totals still fall back to 0,
    /// because there no request value exists to echo and 0 is a truthful "none reported" — the same
    /// convention Core uses (<c>HttpContextExtensions.EmptyResult</c>).
    /// </para>
    /// </summary>
    private static int HeaderInt(HttpResponseMessage response, string name, int fallback = 0) =>
        response.Headers.TryGetValues(name, out var values)
            && int.TryParse(values.FirstOrDefault(), CultureInfo.InvariantCulture, out var parsed)
                ? parsed
                : fallback;
}
