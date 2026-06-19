using System.Text;

namespace Bfs.Iop.Infrastructure.ApiClient;

public abstract class WebApiClientBase
{
    /// <summary>
    /// Optional factory method for the underlying HttpClient
    /// </summary>
    protected abstract Func<Task<HttpClient>>? HttpClientFactory { get; }

    /// <summary>
    /// Request updater interceptors with StringBuilder URL
    /// </summary>
    protected abstract List<Func<HttpClient, HttpRequestMessage, StringBuilder, CancellationToken, Task>> RequestStringBuilderUpdater { get; }

    /// <summary>
    /// Request updater interceptors with string URL
    /// </summary>
    protected abstract List<Func<HttpClient, HttpRequestMessage, string, CancellationToken, Task>> RequestStringUpdater { get; }

    /// <summary>
    /// Response processing interceptors
    /// </summary>
    protected abstract List<Func<HttpClient, HttpResponseMessage, CancellationToken, Task>> ResponseProcessor { get; }

    /// <summary>
    /// Creates the underlying HttpClient using <see cref="HttpClientFactory"/> if provided, otherwise a default instance is provided.
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    protected virtual async Task<HttpClient> CreateHttpClientAsync(CancellationToken _)
    {
        if (HttpClientFactory != null)
        {
            return await HttpClientFactory.Invoke();
        }

        return new HttpClient();
    }

    /// <summary>
    /// Creates a request message to be used for API requests
    /// </summary>
    /// <param name="_"></param>
    /// <returns></returns>
    protected virtual Task<HttpRequestMessage> CreateHttpRequestMessageAsync(CancellationToken _)
    {
        return Task.FromResult(new HttpRequestMessage());
    }

    /// <summary>
    /// Intercepts the API request preparation
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    /// <param name="url"></param>
    /// <param name="cancellationToken"></param>
    protected async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, string url, CancellationToken cancellationToken = default)
    {
        foreach (var updater in RequestStringUpdater)
        {
            await updater.Invoke(client, request, url, cancellationToken);
        }
    }

    /// <summary>
    /// Intercepts the API request preparation
    /// </summary>
    /// <param name="client"></param>
    /// <param name="request"></param>
    /// <param name="urlBuilder"></param>
    /// <param name="cancellationToken"></param>
    protected async Task PrepareRequestAsync(HttpClient client, HttpRequestMessage request, StringBuilder urlBuilder, CancellationToken cancellationToken = default)
    {
        foreach (var updater in RequestStringBuilderUpdater)
        {
            await updater.Invoke(client, request, urlBuilder, cancellationToken);
        }
    }

    /// <summary>
    /// Intercepts the API response processing
    /// </summary>
    /// <param name="client"></param>
    /// <param name="response"></param>
    /// <param name="cancellationToken"></param>
    protected async Task ProcessResponseAsync(HttpClient client, HttpResponseMessage response, CancellationToken cancellationToken)
    {
        foreach (var responseProcessor in ResponseProcessor)
        {
            await responseProcessor.Invoke(client, response, cancellationToken);
        }
    }
}