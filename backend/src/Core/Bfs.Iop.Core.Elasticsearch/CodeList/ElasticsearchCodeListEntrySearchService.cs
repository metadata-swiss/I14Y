using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Commands.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Abstractions.Models.Search;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.Core.Settings;
using Elastic.Clients.Elasticsearch;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Elasticsearch.CodeList;

/// <summary>
/// Elasticsearch-backed <see cref="ICodeListEntrySearchService"/>. Near-copy of the Lucene service with
/// the query execution swapped to ES; the auth check, DB hydration and ancestor-path building are reused
/// unchanged so results are identical in shape.
/// </summary>
internal sealed class ElasticsearchCodeListEntrySearchService : ICodeListEntrySearchService
{
    private static readonly JsonSerializerOptions FilterJsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly ElasticsearchClient _client;
    private readonly IIopConceptsService _iopConceptsService;
    private readonly IMediator _mediator;
    private readonly ILogger<ElasticsearchCodeListEntrySearchService> _logger;
    private readonly string _index;

    public ElasticsearchCodeListEntrySearchService(
        ElasticsearchClient client,
        IOptions<ElasticsearchOptions> options,
        IIopConceptsService iopConceptsService,
        IMediator mediator,
        ILogger<ElasticsearchCodeListEntrySearchService> logger)
    {
        _client = client;
        _iopConceptsService = iopConceptsService;
        _mediator = mediator;
        _logger = logger;
        _index = options.Value.CodeListIndexName;
    }

    public async Task<PagedResult<CodeListEntrySearchResultEntryModel>> Search(
        Guid conceptId,
        string language,
        string? query,
        List<string> filters,
        bool addCodeListEntriesPaths,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        // Ensure the user is allowed to access the concept (throws otherwise).
        _ = await _iopConceptsService.GetIopConcept(conceptId, false, cancellationToken);

        var filterConfiguration = new FilterConfigurationModel { Filters = [] };
        try
        {
            filterConfiguration = await _mediator.Send(new GetFilterConfigurationCommand(conceptId), cancellationToken);
        }
        catch
        {
            // no filter configuration -> use the default (empty)
        }

        var parsedFilters = filters
            .Select(f => JsonSerializer.Deserialize<FilterInputModel>(f, FilterJsonOptions))
            .Where(f => f is not null)
            .Cast<FilterInputModel>()
            .ToArray();

        var from = (page - 1) * pageSize;
        var size = pageSize == int.MaxValue ? 10000 : pageSize;

        var body = CodeListQueryBuilder.BuildSearchBody(conceptId, language, query, parsedFilters, filterConfiguration, from, size);

        var response = await EsRest.SendAsync(
            _client,
            Elastic.Transport.HttpMethod.POST,
            $"/{_index}/_search",
            JsonSerializer.Serialize(body),
            cancellationToken);

        if (response.ApiCallDetails?.HasSuccessfulStatusCode != true)
        {
            _logger.LogError("Elasticsearch codelist search failed (HTTP {Status}): {Body}",
                response.ApiCallDetails?.HttpStatusCode, response.Body);
        }

        var (idsInOrder, scores, total) = ParseHits(EsRest.ReadBodyOrThrow(response, "codelist search"));

        var models = (await _iopConceptsService.GetCodeListEntriesByIds(idsInOrder, cancellationToken)).ToList();

        var entryPaths = addCodeListEntriesPaths
            ? await CreatePaths(conceptId, models, cancellationToken)
            : [];

        var results = idsInOrder
            .Where(id => models.Any(m => m.Id == id))
            .Select(id => new CodeListEntrySearchResultEntryModel
            {
                Entry = models.Single(m => m.Id == id),
                Score = scores[id],
                Path = addCodeListEntriesPaths ? entryPaths[id] : [],
            });

        return new PagedResult<CodeListEntrySearchResultEntryModel>
        {
            Results = results,
            Page = page,
            PageSize = pageSize == int.MaxValue ? total : pageSize,
            TotalCount = total,
        };
    }

    private static (List<Guid> Ids, Dictionary<Guid, float> Scores, int Total) ParseHits(string json)
    {
        var ids = new List<Guid>();
        var scores = new Dictionary<Guid, float>();

        using var doc = JsonDocument.Parse(json);
        if (!doc.RootElement.TryGetProperty("hits", out var hitsRoot))
        {
            return (ids, scores, 0);
        }

        var total = hitsRoot.TryGetProperty("total", out var totalEl) && totalEl.TryGetProperty("value", out var totalValue)
            ? totalValue.GetInt32()
            : 0;

        if (hitsRoot.TryGetProperty("hits", out var hits))
        {
            foreach (var hit in hits.EnumerateArray())
            {
                if (!hit.TryGetProperty("_source", out var src)
                    || !src.TryGetProperty(EsCodeListFields.Id, out var idEl)
                    || !Guid.TryParse(idEl.GetString(), out var id))
                {
                    continue;
                }

                var score = hit.TryGetProperty("_score", out var scoreEl) && scoreEl.ValueKind == JsonValueKind.Number
                    ? scoreEl.GetSingle()
                    : 0f;

                if (scores.TryAdd(id, score))
                {
                    ids.Add(id);
                }
            }
        }

        return (ids, scores, total);
    }

    private async Task<Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>> CreatePaths(
        Guid conceptId,
        IEnumerable<CodeListEntryModel> codeListEntryModels,
        CancellationToken cancellationToken)
    {
        var paths = new Dictionary<Guid, IEnumerable<CodeListEntrySearchResultPathModel>>();

        foreach (var model in codeListEntryModels)
        {
            paths[model.Id] = await CreatePath([], conceptId, model, cancellationToken);
        }

        return paths;
    }

    private async Task<IEnumerable<CodeListEntrySearchResultPathModel>> CreatePath(
        List<CodeListEntrySearchResultPathModel> paths,
        Guid conceptId,
        CodeListEntryModel model,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(model.ParentCode))
        {
            var parent = (await _iopConceptsService.GetCodeListEntriesByCodes(conceptId, [model.ParentCode], cancellationToken)).Single();
            await CreatePath(paths, conceptId, parent, cancellationToken);
        }

        paths.Add(new CodeListEntrySearchResultPathModel { Code = model.Code, Name = model.Name, ParentCode = model.ParentCode });
        return paths;
    }
}
