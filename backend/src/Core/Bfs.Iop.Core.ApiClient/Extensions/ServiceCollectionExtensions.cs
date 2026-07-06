using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Bfs.Iop.Core.ApiClient.Extensions;

/// <summary>
/// Adds the business related registrations
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the API client
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiBaseAddress">The URL where the client is connecting to</param>
    /// <returns></returns>
    public static IServiceCollection AddIopCoreApiClient(this IServiceCollection services, string apiBaseAddress)
    {
        services.TryAddTransient(sp => new IopCoreApiClientSupport(apiBaseAddress, sp.GetService<ITokenRetriever>()));

        services.TryAddTransient(sp => new IopCoreApiClient(sp.GetRequiredService<IopCoreApiClientSupport>()));

        services.AddTransient<IIopCoreApiClient>(sp => sp.GetRequiredService<IopCoreApiClient>());

        return services;
    }
}