using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.AuditTrail.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditTrailApiClient(this IServiceCollection services, string baseAddress)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        if (string.IsNullOrWhiteSpace(baseAddress))
        {
            throw new ArgumentException("Base address cannot be null or whitespace.", nameof(baseAddress));
        }

        services.AddHttpClient<IAuditTrailApiClient, AuditTrailApiClient>(client =>
        {
            client.BaseAddress = new Uri(baseAddress);
        });

        return services;
    }
}
