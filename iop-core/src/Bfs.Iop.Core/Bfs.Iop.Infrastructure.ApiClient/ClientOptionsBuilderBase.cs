using System.Text;

namespace Bfs.Iop.Infrastructure.ApiClient;

public abstract class ClientOptionsBuilderBase : IClientOptionsBuilder
{
    /// <summary>
    /// Request string interceptors
    /// </summary>
    public List<Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task>> RequestStringUpdater { get; } = new();

    /// <summary>
    /// Request string builder interceptors
    /// </summary>
    public List<Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task>> RequestStringBuilderUpdater { get; } = new();

    /// <summary>
    /// Response interceptors
    /// </summary>
    public List<Func<HttpClient, HttpResponseMessage, CancellationToken, Task>> ResponseProcessor { get; } = new();

    /// <summary>
    /// Options HttpClient factory method
    /// </summary>
    public Func<Task<HttpClient>>? HttpClientFactory { get; private set; }

    /// <summary>
    /// Adds a request string interceptor
    /// </summary>
    /// <param name="requestUpdater"></param>
    public void AddPrepareRequest(Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task> requestUpdater)
    {
        RequestStringUpdater.Add(requestUpdater);
    }

    /// <summary>
    /// Adds a request string builder interceptor
    /// </summary>
    /// <param name="requestUpdater"></param>
    public void AddPrepareRequest(Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task> requestUpdater)
    {
        RequestStringBuilderUpdater.Add(requestUpdater);
    }

    /// <summary>
    /// Adds a response processing interceptor
    /// </summary>
    /// <param name="responseProcessor"></param>
    public void AddProcessResponse(Func<HttpClient, HttpResponseMessage, CancellationToken, Task> responseProcessor)
    {
        ResponseProcessor.Add(responseProcessor);
    }

    /// <summary>
    /// Sets the HttpClient factory method to use
    /// </summary>
    /// <param name="httpClientFactory"></param>
    public void SetHttpClientFactory(Func<Task<HttpClient>> httpClientFactory)
    {
        HttpClientFactory = httpClientFactory;
    }
}
