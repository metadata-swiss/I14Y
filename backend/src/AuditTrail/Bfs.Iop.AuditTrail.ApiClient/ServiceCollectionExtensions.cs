using Bfs.Iop.AuditTrail.ApiClient.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.AuditTrail.ApiClient;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAuditTrailApiClient(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services
            .AddSingleton<IValidateOptions<AuditTrailOptions>, AuditTrailOptionsValidator>()
            .AddOptionsWithValidateOnStart<AuditTrailOptions>()
            .Bind(configuration.GetSection(AuditTrailOptions.SectionName));

        services.AddHttpClient<IAuditTrailApiClient, AuditTrailApiClient>((sp, client) =>
        {
            var options = sp.GetRequiredService<IOptions<AuditTrailOptions>>().Value;
            client.BaseAddress = new Uri(options.BaseUrl);
        });

        return services;
    }
}
