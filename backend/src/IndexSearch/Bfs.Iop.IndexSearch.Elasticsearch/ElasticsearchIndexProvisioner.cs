using System.Globalization;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using Bfs.Iop.IndexSearch.Elasticsearch.CodeList;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Elasticsearch;

public sealed record PreparedIndices(string Catalog, string CodeList);

public sealed class ElasticsearchIndexProvisioner
{
    public const string HttpClientName = "IndexSearch.Elasticsearch.Admin";

    private const string Json = "application/json";
    private const string StampFormat = "yyyyMMddHHmmssfff";

    private readonly HttpClient _client;
    private readonly IndexNames _names;
    private readonly TimeProvider _time;
    private readonly ILogger<ElasticsearchIndexProvisioner> _logger;

    public ElasticsearchIndexProvisioner(
        HttpClient client,
        IndexNames names,
        ILogger<ElasticsearchIndexProvisioner> logger,
        TimeProvider? time = null)
    {
        _client = client;
        _names = names;
        _logger = logger;
        _time = time ?? TimeProvider.System;
    }

    public async Task<bool> ExistsAsync(string index, CancellationToken cancellationToken = default)
    {
        using var request = new HttpRequestMessage(HttpMethod.Head, $"/{index}");

        var response = await _client.SendAsync(request, cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return false;
        }

        if (response.IsSuccessStatusCode)
        {
            return true;
        }

        throw new HttpRequestException(
            $"Could not determine whether index ''{index}'' exists: {(int)response.StatusCode}.");
    }

    public async Task CreateIfMissingAsync(CancellationToken cancellationToken = default)
    {
        var created = new List<(string Alias, string Index)>();

        foreach (var (alias, mapping) in Both())
        {
            if (await ExistsAsync(alias, cancellationToken))
            {
                _logger.LogInformation("{Alias} already resolves; its mapping is left as it is.", alias);
                continue;
            }

            var index = $"{alias}-{Stamp()}";

            await CreateAsync(index, mapping, cancellationToken);

            created.Add((alias, index));
        }

        if (created.Count > 0)
        {
            await PublishAsync(created, cancellationToken);
        }
    }

    public async Task<PreparedIndices> PrepareAsync(CancellationToken cancellationToken = default)
    {
        await SweepOrphansAsync(cancellationToken);

        var stamp = Stamp();

        var catalog = $"{_names.Catalog}-{stamp}";
        var codeList = $"{_names.CodeList}-{stamp}";

        await CreateAsync(catalog, CatalogIndexMapping.BuildCreateIndexJson(), cancellationToken);
        await CreateAsync(codeList, CodeListIndexMapping.BuildCreateIndexJson(), cancellationToken);

        return new PreparedIndices(catalog, codeList);
    }

    public async Task PublishAsync(PreparedIndices prepared, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prepared);

        await PublishAsync(
            [(_names.Catalog, prepared.Catalog), (_names.CodeList, prepared.CodeList)],
            cancellationToken);
    }

    public async Task DiscardAsync(PreparedIndices prepared, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(prepared);

        await DeleteAsync(prepared.Catalog, cancellationToken);
        await DeleteAsync(prepared.CodeList, cancellationToken);
    }

    public async Task SweepOrphansAsync(CancellationToken cancellationToken = default)
    {
        await SweepOrphansAsync(_names.Catalog, cancellationToken);
        await SweepOrphansAsync(_names.CodeList, cancellationToken);
    }

    public async Task ForceMergeAsync(CancellationToken cancellationToken = default)
    {
        await ForceMergeAsync(_names.Catalog, cancellationToken);
        await ForceMergeAsync(_names.CodeList, cancellationToken);
    }

    private async Task ForceMergeAsync(string index, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Post,
            $"/{index}/_forcemerge?max_num_segments=1&wait_for_completion=false");

        var response = await _client.SendAsync(request, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            // Logged, not thrown, for the same reason: the documents are indexed either way.
            var detail = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogWarning(
                "Could not start a force merge of {Index}: {Status} {Detail}. The index keeps the "
                + "segments the pass left; search is unaffected.",
                index,
                (int)response.StatusCode,
                detail);

            return;
        }

        _logger.LogInformation("Started a background force merge of {Index}.", index);
    }

    private string Stamp() => _time.GetUtcNow().ToString(StampFormat, CultureInfo.InvariantCulture);

    private async Task SweepOrphansAsync(string alias, CancellationToken cancellationToken)
    {
        var members = await AliasMembersAsync(alias, cancellationToken);
        var generations = await GenerationsAsync(alias, cancellationToken);

        foreach (var orphan in generations.Except(members, StringComparer.Ordinal))
        {
            await DeleteAsync(orphan, cancellationToken);

            _logger.LogWarning(
                "Dropped {Index}, left behind by a pass that never published it.",
                orphan);
        }
    }

    private async Task<IReadOnlyList<string>> GenerationsAsync(string alias, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/_cat/indices/{alias}-*?h=index&format=json", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowAsync(response, $"list the indices under '{alias}'", cancellationToken);
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        return
        [
            .. document.RootElement.EnumerateArray()
                .Select(x => x.TryGetProperty("index", out var name) ? name.GetString() : null)
                .Where(x => x is not null && IsGeneration(alias, x))
                .Select(x => x!),
        ];
    }

    private static bool IsGeneration(string alias, string index)
    {
        var prefix = $"{alias}-";

        if (!index.StartsWith(prefix, StringComparison.Ordinal))
        {
            return false;
        }

        var stamp = index[prefix.Length..];

        return stamp.Length == StampFormat.Length && stamp.All(char.IsAsciiDigit);
    }

    private (string Alias, string Mapping)[] Both() =>
    [
        (_names.Catalog, CatalogIndexMapping.BuildCreateIndexJson()),
        (_names.CodeList, CodeListIndexMapping.BuildCreateIndexJson()),
    ];

    // Every alias moves in one _aliases request, and nothing is deleted until it has. Publishing them
    // one at a time meant a failure between the two left a new catalog paired with the previous code
    // list, with the superseded catalog already dropped and no way back to a consistent pair.
    private async Task PublishAsync(
        IReadOnlyList<(string Alias, string Index)> generations,
        CancellationToken cancellationToken)
    {
        var actions = new List<object>();
        var superseded = new List<string>();

        foreach (var (alias, index) in generations)
        {
            var previous = await AliasMembersAsync(alias, cancellationToken);

            // An alias cannot share a name with an index, and before the alias existed the name was
            // one. remove_index belongs inside the transaction so even that first swap is atomic.
            if (previous.Count == 0 && await ExistsAsync(alias, cancellationToken))
            {
                _logger.LogWarning(
                    "Replacing the concrete index {Alias} with an alias of the same name. It holds "
                    + "nothing that is not rebuilt from the database.",
                    alias);

                actions.Add(new Dictionary<string, object?>
                {
                    ["remove_index"] = new Dictionary<string, object?> { ["index"] = alias },
                });
            }

            foreach (var member in previous.Where(x => !string.Equals(x, index, StringComparison.Ordinal)))
            {
                actions.Add(new Dictionary<string, object?>
                {
                    ["remove"] = new Dictionary<string, object?> { ["index"] = member, ["alias"] = alias },
                });

                superseded.Add(member);
            }

            actions.Add(new Dictionary<string, object?>
            {
                ["add"] = new Dictionary<string, object?> { ["index"] = index, ["alias"] = alias },
            });
        }

        var body = JsonSerializer.Serialize(new Dictionary<string, object?> { ["actions"] = actions });

        using var content = new StringContent(body, Encoding.UTF8, Json);

        var response = await _client.PostAsync("/_aliases", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var moved = string.Join(", ", generations.Select(x => $"{x.Alias} -> {x.Index}"));

            await ThrowAsync(response, $"move the aliases ({moved})", cancellationToken);
        }

        foreach (var (alias, index) in generations)
        {
            _logger.LogInformation("Alias {Alias} now serves {Index}.", alias, index);
        }

        // Only once every alias has moved, so a failure here leaves disk to reclaim rather than a
        // generation nothing points at.
        foreach (var member in superseded)
        {
            await DeleteAsync(member, cancellationToken);

            _logger.LogInformation("Dropped the superseded index {Index}.", member);
        }
    }

    private async Task<IReadOnlyList<string>> AliasMembersAsync(string alias, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/_alias/{alias}", cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return [];
        }

        if (!response.IsSuccessStatusCode)
        {
            await ThrowAsync(response, $"read alias '{alias}'", cancellationToken);
        }

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));

        return [.. document.RootElement.EnumerateObject().Select(x => x.Name)];
    }

    private async Task DeleteAsync(string index, CancellationToken cancellationToken)
    {
        var response = await _client.DeleteAsync($"/{index}", cancellationToken);

        if (!response.IsSuccessStatusCode && response.StatusCode != HttpStatusCode.NotFound)
        {
            await ThrowAsync(response, $"delete index '{index}'", cancellationToken);
        }
    }

    private async Task CreateAsync(string index, string mapping, CancellationToken cancellationToken)
    {
        using var content = new StringContent(mapping, Encoding.UTF8, Json);

        var response = await _client.PutAsync($"/{index}", content, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ThrowAsync(response, $"create index '{index}'", cancellationToken);
        }

        _logger.LogInformation("Created index {Index}.", index);
    }

    private static async Task ThrowAsync(
        HttpResponseMessage response,
        string what,
        CancellationToken cancellationToken)
    {
        var detail = await response.Content.ReadAsStringAsync(cancellationToken);

        throw new HttpRequestException($"Could not {what}: {(int)response.StatusCode} {detail}");
    }
}
