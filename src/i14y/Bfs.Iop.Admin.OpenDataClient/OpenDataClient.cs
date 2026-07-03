using MapsterMapper;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.OpenDataClient;

internal class OpenDataClient : AbstractOpenDataClient
{
    private readonly string _apiBaseUrl;
    private readonly HttpClient _client;

    public OpenDataClient(string apiBaseUrl, string linkBaseUri, HttpClient client, IMapper mapper) : base(linkBaseUri, mapper)
    {
        _apiBaseUrl = apiBaseUrl?.TrimEnd('/') ?? throw new ArgumentNullException(nameof(apiBaseUrl));
        _client = client ?? throw new ArgumentNullException(nameof(client));
    }

    protected override async Task<string> ExecuteRequest(string query, int? from, int? pageSize, CancellationToken cancellationToken)
    {
        using var request = BuildRequest(query, from, pageSize);
        using var response = await _client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);

        var status = (int)response.StatusCode;
        if (status != 200 && status != 206) throw new HttpRequestException($"Received unexpected HTTP status code ({status}) when issuing request {request.RequestUri}");

        using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        using var reader = new StreamReader(stream, leaveOpen: true);
        return await reader.ReadToEndAsync(cancellationToken);
    }

    private HttpRequestMessage BuildRequest(string searchTerm, int? from, int? hitsPerPage)
    {
        var requestUri = $"{_apiBaseUrl}?q={(searchTerm ?? string.Empty).Replace(" ", "+")}&sort=score desc, metadata_modified desc";
        if (from.HasValue) requestUri = requestUri + $"&start={from}";
        if (hitsPerPage.HasValue) requestUri = requestUri + $"&rows={hitsPerPage}";

        var request = new HttpRequestMessage
        {
            Method = new HttpMethod("GET"),
            RequestUri = new Uri(requestUri, UriKind.RelativeOrAbsolute)
        };
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(MediaTypeNames.Application.Json));

        // ckan.opendata.swiss returns 403 when no User-Agent is provided.
        request.Headers.UserAgent.ParseAdd("I14Y (contact: i14y@bfs.admin.ch)");

        return request;
    }
}