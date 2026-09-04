using Bfs.Iop.AuditTrail.Business.Configuration;
using Bfs.Iop.AuditTrail.Business.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.AuditTrail.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddSingleton<IValidateOptions<GitOptions>, GitOptionsValidator>()
            .AddOptionsWithValidateOnStart<GitOptions>()
            .Bind(configuration.GetSection(nameof(GitOptions)));

        services.AddSingleton<IGitWrapper, GitWrapper>();
        services.AddScoped<IResourceTrackerService, GitResourceTrackerService>();

        // Services need for processing the commits:
        services.AddSingleton<GitCommitProcessorService>();
        services.AddHostedService<GitCommitBackgroundService>();

        return services;
    }
}
