using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bfs.Iop.IndexSearch.ApiClient.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIndexSearchApiClient(
        this IServiceCollection services,
        string apiBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseAddress);

        services.AddHttpClient(IndexSearchApiClientSupport.HttpClientName);

        services.TryAddTransient(sp => new IndexSearchApiClientSupport(
            apiBaseAddress,
            sp.GetService<ITokenRetriever>(),
            sp.GetRequiredService<IHttpClientFactory>()));

        services.TryAddTransient(sp =>
            new IndexSearchApiClient(sp.GetRequiredService<IndexSearchApiClientSupport>()));

        services.AddTransient<IIndexSearchApiClient>(sp =>
            sp.GetRequiredService<IndexSearchApiClient>());

        return services;
    }
}
