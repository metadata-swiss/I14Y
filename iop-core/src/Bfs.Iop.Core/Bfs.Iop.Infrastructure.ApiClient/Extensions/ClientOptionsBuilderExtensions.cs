using System.Net.Http.Headers;

namespace Bfs.Iop.Infrastructure.ApiClient.Extensions;

public static class ClientOptionsBuilderExtensions
{
    /// <summary>
    /// Registers a resolver for the bearer authentication token to be used for setting bearer authentication headers on API requests.
    /// </summary>
    /// <param name="optionsBuilder">This options builder on which to register the auth headers.</param>
    /// <param name="accessTokenResolver">Function to retrieve the bearer token.</param>
    /// <typeparam name="T">Client options builder type.</typeparam>
    /// <returns>This options builder instance for chaining.</returns>
    public static T AddBearerAuthentication<T>(this T optionsBuilder, Func<Task<string>> accessTokenResolver)
        where T : IClientOptionsBuilder
    {
        async Task PrepareRequest(HttpClient httpClient, HttpRequestMessage message, string url, CancellationToken cancellationToken)
        {
            var accessToken = await accessTokenResolver.Invoke();
            if (!string.IsNullOrEmpty(accessToken))
            {
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }
        }

        optionsBuilder.AddPrepareRequest(PrepareRequest);

        return optionsBuilder;
    }

    /// <summary>
    /// Sets the base url of the web api.
    /// </summary>
    /// <param name="optionsBuilder">This options builder on which to set the base url.</param>
    /// <param name="baseUrl">Web API base url.</param>
    /// <typeparam name="T">Client options builder type.</typeparam>
    /// <returns>This options builder instance for chaining.</returns>
    public static T SetBaseUrl<T>(this T optionsBuilder, string baseUrl)
        where T : IClientOptionsBuilder
    {
        Task PrepareRequest(HttpClient httpClient, HttpRequestMessage message, string url, CancellationToken cancellationToken)
        {
            httpClient.BaseAddress = new Uri(baseUrl);

            return Task.CompletedTask;
        }

        optionsBuilder.AddPrepareRequest(PrepareRequest);

        return optionsBuilder;
    }
}
