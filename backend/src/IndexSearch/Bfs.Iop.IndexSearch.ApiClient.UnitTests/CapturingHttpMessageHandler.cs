using System.Net;

namespace Bfs.Iop.IndexSearch.ApiClient.UnitTests;

/// <summary>
/// Captures the outgoing request and returns a canned response, so tests can assert on the URL,
/// headers and body the client actually produces rather than on a mock of the client itself.
/// </summary>
internal sealed class CapturingHttpMessageHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _statusCode;
    private readonly string _responseJson;
    private readonly IReadOnlyDictionary<string, string> _responseHeaders;

    public CapturingHttpMessageHandler(
        string responseJson = "[]",
        HttpStatusCode statusCode = HttpStatusCode.OK,
        IReadOnlyDictionary<string, string>? responseHeaders = null)
    {
        _responseJson = responseJson;
        _statusCode = statusCode;
        _responseHeaders = responseHeaders ?? new Dictionary<string, string>();
    }

    public HttpRequestMessage? Request { get; private set; }

    public string? RequestBody { get; private set; }

    /// <summary>The query string parsed into repeated key/value pairs, preserving duplicates.</summary>
    public IReadOnlyList<KeyValuePair<string, string>> QueryParameters
    {
        get
        {
            var query = Request?.RequestUri?.Query;

            if (string.IsNullOrEmpty(query))
            {
                return [];
            }

            return [.. query.TrimStart('?')
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(pair => pair.Split('=', 2))
                .Select(parts => new KeyValuePair<string, string>(
                    Uri.UnescapeDataString(parts[0]),
                    parts.Length > 1 ? Uri.UnescapeDataString(parts[1]) : string.Empty))];
        }
    }

    public IEnumerable<string> ValuesOf(string name) =>
        QueryParameters.Where(x => x.Key == name).Select(x => x.Value);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Request = request;

        if (request.Content is not null)
        {
            RequestBody = await request.Content.ReadAsStringAsync(cancellationToken);
        }

        var response = new HttpResponseMessage(_statusCode)
        {
            Content = new StringContent(_responseJson, System.Text.Encoding.UTF8, "application/json"),
        };

        foreach (var (name, value) in _responseHeaders)
        {
            response.Headers.TryAddWithoutValidation(name, value);
        }

        return response;
    }
}
