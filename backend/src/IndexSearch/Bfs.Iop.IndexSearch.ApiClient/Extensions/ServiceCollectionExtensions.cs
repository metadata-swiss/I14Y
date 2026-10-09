using Bfs.Iop.IndexSearch.ApiClient.Authentication;
using Bfs.Iop.IndexSearch.ApiClient.Health;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bfs.Iop.IndexSearch.ApiClient.Extensions;

public static class ServiceCollectionExtensions
{
    internal const string HealthCheckName = "IndexSearch";

    public static IServiceCollection AddIndexSearchApiClient(
        this IServiceCollection services,
        string apiBaseAddress)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentException.ThrowIfNullOrWhiteSpace(apiBaseAddress);

        services.AddHttpClient(IndexSearchApiClientSupport.HttpClientName);

        services.AddHttpContextAccessor();
        services.TryAddTransient<ITokenRetriever, RequestUserTokenRetriever>();

        services.TryAddTransient(sp => new IndexSearchApiClientSupport(
            apiBaseAddress,
            sp.GetService<ITokenRetriever>(),
            sp.GetRequiredService<IHttpClientFactory>()));

        services.TryAddTransient(sp =>
            new IndexSearchApiClient(sp.GetRequiredService<IndexSearchApiClientSupport>()));

        services.AddTransient<IIndexSearchApiClient>(sp =>
            sp.GetRequiredService<IndexSearchApiClient>());

        // Two checks under one name throw when the health service is built, so a second call to this
        // method must not register it again. The transient doubles as the marker for that.
        if (services.All(x => x.ServiceType != typeof(IndexSearchApiClientHealthCheck)))
        {
            services.AddTransient<IndexSearchApiClientHealthCheck>();
            services.AddHealthChecks().AddCheck<IndexSearchApiClientHealthCheck>(HealthCheckName);
        }

        return services;
    }
}
