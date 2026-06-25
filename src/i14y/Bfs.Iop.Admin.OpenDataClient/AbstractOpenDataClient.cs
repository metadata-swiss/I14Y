using AutoMapper;
using Bfs.Iop.Admin.OpenDataClient.PackageSearch;
using Bfs.Iop.Admin.Models.OpenData;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.OpenDataClient;

internal abstract class AbstractOpenDataClient : IOpenDataIndex
{
    protected readonly string _linkBaseUri;
    private readonly IMapper _mapper;

    protected AbstractOpenDataClient(string linkBaseUri, IMapper mapper)
    {
        _linkBaseUri = linkBaseUri?.TrimEnd('/') ?? throw new ArgumentNullException($"{nameof(linkBaseUri)}");
        _mapper = mapper ?? throw new ArgumentNullException($"{nameof(mapper)}");
    }

    public async Task<OpenDataSearchResult> Search(string query, string culture, int? page, int? pageSize, CancellationToken cancellationToken)
    {
        var from = page.HasValue && pageSize.HasValue ? (page.Value - 1) * pageSize.Value : (int?)null;

        string rawResponse = await ExecuteRequest(query, from, pageSize, cancellationToken);
        var response = Newtonsoft.Json.JsonConvert.DeserializeObject<SearchResponse>(rawResponse);

        var result = _mapper.Map<OpenDataSearchResult>(
            response?.Result,
            opt => opt.Items["LinkBaseUri"] = $"{_linkBaseUri}/"
        );
        result.From = from ?? 0;
        result.To = result.From + result.Count;

        return result;
    }

    protected abstract Task<string> ExecuteRequest(string query, int? from, int? pageSize, CancellationToken cancellationToken);
}