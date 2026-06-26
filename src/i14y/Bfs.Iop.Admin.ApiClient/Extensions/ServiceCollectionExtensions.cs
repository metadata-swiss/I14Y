using Bfs.Iop.Admin.Api.ApiClient;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.ApiClient.Extensions;

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
    public static IServiceCollection AddBfsIopAdminApiClient(this IServiceCollection services, Uri apiBaseAddress)
    {
        services.AddTransient(sp => new BfsIopAdminApiClientSupport(apiBaseAddress.ToString(), () => Task.FromResult(sp.GetRequiredService<IAccessTokenProvider>().AccessToken)));
        services.AddTransient(sp => new IopAdminApiClient(sp.GetRequiredService<BfsIopAdminApiClientSupport>()));
        services.AddTransient<IIopAdminApiClient>(sp => sp.GetRequiredService<IopAdminApiClient>());           

        return services;
    }
}