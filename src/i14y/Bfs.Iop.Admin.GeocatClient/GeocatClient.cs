using MapsterMapper;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.GeocatClient;

internal class GeocatClient : AbstractGeocatClient
{
    private readonly HttpClient _client;

    public GeocatClient(string baseUri, HttpClient client, IMapper mapper) : base(baseUri, mapper)
    => _client = client ?? throw new ArgumentNullException(nameof(client));

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
        var requestScoreFunctions = "[{\"filter\": {\"exists\": {\"field\": \"parentUuid\"}}, \"weight\": 0.3}, {\"filter\": {\"match\": {\"cl_status.key\": \"obsolete\"}},\"weight\": 0.2}, {\"filter\": {\"match\": {\"cl_status.key\": \"superseded\"}},\"weight\": 0.3}, {\"gauss\": {\"dateStamp\": {\"scale\": \"365d\",\"offset\": \"90d\",\"decay\": 0.5}}}]";
        var requestIncludes = "[\"uuid\", \"resource*\"]";
        var requestQuery = string.IsNullOrEmpty(searchTerm)
            ? $"{{\"bool\": {{\"must\": [{{\"terms\": {{\"isTemplate\": [\"n\"]}}}}]}}}}"
            : $"{{\"bool\": {{\"must\": [{{\"query_string\": {{\"query\": \"(any.\\\\*:({searchTerm}) OR any.common:({searchTerm}) OR resourceTitleObject.\\\\*:({searchTerm})^2 OR resourceTitleObject.\\\\*:\\\"{searchTerm}\\\"^6)\",\"default_operator\": \"AND\"}}}}, {{\"terms\": {{\"isTemplate\": [\"n\"]}}}}]}}}}";
        var requestContent = $"{{\"from\": {from},\"size\": {hitsPerPage},\"sort\": [\"_score\"],\"query\": {{\"function_score\": {{\"boost\": \"5\",\"functions\": {requestScoreFunctions},\"score_mode\": \"multiply\",\"query\": {requestQuery}}}}},\"_source\": {{\"includes\": {requestIncludes}}},\"track_total_hits\": true}}";

        var request = new HttpRequestMessage
        {
            Method = new HttpMethod("POST"),
            RequestUri = new Uri($"{_baseUri}/api/search/records/_search", UriKind.RelativeOrAbsolute),
            Content = new StringContent(requestContent, Encoding.UTF8, MediaTypeNames.Application.Json)
        };
        request.Headers.Accept.Add(MediaTypeWithQualityHeaderValue.Parse(MediaTypeNames.Application.Json));

        return request;
    }
}