using System.Text;

namespace Bfs.Iop.Infrastructure.ApiClient;

public interface IClientOptionsBuilder
{
    /// <summary>
    /// Adds a request string interceptor
    /// </summary>
    /// <param name="requestUpdater"></param>
    void AddPrepareRequest(Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task> requestUpdater);

    /// <summary>
    /// Adds a request string builder interceptor
    /// </summary>
    /// <param name="requestUpdater"></param>
    void AddPrepareRequest(Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task> requestUpdater);

    /// <summary>
    /// Adds a response processing interceptor
    /// </summary>
    /// <param name="responseProcessor"></param>
    void AddProcessResponse(Func<HttpClient, HttpResponseMessage, CancellationToken, Task> responseProcessor);

    /// <summary>
    /// Sets the HttpClient factory method to use
    /// </summary>
    /// <param name="httpClientFactory"></param>
    void SetHttpClientFactory(Func<Task<HttpClient>> httpClientFactory);
}
