using Bfs.Iop.Admin.GeocatClient.ElasticSearch;
using Bfs.Iop.Admin.Models.Geocat;
using Mapster;
using MapsterMapper;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.GeocatClient;

internal abstract class AbstractGeocatClient : IGeocatIndex
{
    protected readonly string _baseUri;
    private readonly IMapper _mapper;

    protected AbstractGeocatClient(string baseUri, IMapper mapper)
    {
        _baseUri = baseUri?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(baseUri));
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    public async Task<GeocatSearchResult> Search(
        string query,
        string culture,
        int? page,
        int? pageSize,
        CancellationToken cancellationToken)
    {
        var from = page.HasValue && pageSize.HasValue
            ? (page.Value - 1) * pageSize.Value
            : (int?)null;

        string rawResponse = await ExecuteRequest(query, from, pageSize, cancellationToken);
        var response = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResponse>(rawResponse);

        using var scope = new MapContextScope();

        MapContext.Current.Parameters["LinkBaseUri"] =
            $"{_baseUri}/{ConvertToGeocatCulture(culture)}/catalog.search#/metadata/";

        var result = _mapper.Map<GeocatSearchResult>(response.Hits);

        result.From = from ?? 0;
        result.To = result.From + result.Count;

        return result;
    }

    protected abstract Task<string> ExecuteRequest(
        string query,
        int? from,
        int? pageSize,
        CancellationToken cancellationToken);

    private static string ConvertToGeocatCulture(string culture)
        => culture switch
        {
            "en" => "eng",
            "fr" => "fre",
            "it" => "ita",
            _ => "ger"
        };
}