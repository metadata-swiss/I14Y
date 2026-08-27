using Bfs.Iop.IndexSearch.ApiClient.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.IndexSearch.ApiClient.Extensions;

/// <summary>
/// Adds the IndexSearch API client registrations.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the API client.
    /// <para>
    /// The base address is passed in rather than read from configuration here, matching
    /// <c>AddIopCoreApiClient</c>: the composition root owns config, and a caller that forgets the
    /// setting should fail there — loudly — instead of getting a client pointed at nothing.
    /// </para>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiBaseAddress">The URL where the client is connecting to.</param>
    /// <param name="secret">Pre-shared secret sent as X-Api-Key; must match the service's IndexSearch:Secret.</param>
    public static IServiceCollection AddIndexSearchApiClient(
        this IServiceCollection services,
        string apiBaseAddress,
        string secret)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseAddress);

        services.AddHttpClient<IIndexSearchApiClient, IndexSearchApiClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(30);

            // A header, not a query parameter: query strings land in access and proxy logs. The
            // receiving service also already redacts this header name from its request logs.
            client.DefaultRequestHeaders.Add("X-Api-Key", secret ?? string.Empty);
        });

        return services;
    }

    /// <summary>
    /// Registers the read client for <c>/api/Search*</c> and code-list search.
    /// <para>
    /// Requires an <see cref="IIndexSearchTokenProvider"/> to be registered by the caller. Reads are
    /// user-scoped, so forwarding the caller's token is not optional: without it a signed-in user
    /// silently gets public-only results with HTTP 200.
    /// <para>
    /// Note the limit of that guarantee: the handler — and therefore the provider — is resolved
    /// lazily on the first request, not at startup. A missing registration throws on the first
    /// search rather than failing the host, so it is loud but late. Register the provider in the
    /// same conditional block as this call so the two cannot diverge.
    /// </para>
    /// </para>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiBaseAddress">The URL where the client is connecting to.</param>
    public static IServiceCollection AddIndexSearchSearchClient(
        this IServiceCollection services,
        string apiBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseAddress);

        services.AddTransient<IndexSearchBearerTokenHandler>();

        services.AddHttpClient<IIndexSearchSearchClient, IndexSearchSearchClient>(client =>
        {
            client.BaseAddress = new Uri(apiBaseAddress.TrimEnd('/') + "/");

            // Longer than the write timeout: a user is waiting for these, and a facet count over a
            // large corpus is legitimately slower than queueing an index event.
            client.Timeout = TimeSpan.FromSeconds(60);
        })
        .AddHttpMessageHandler<IndexSearchBearerTokenHandler>();

        return services;
    }
}
