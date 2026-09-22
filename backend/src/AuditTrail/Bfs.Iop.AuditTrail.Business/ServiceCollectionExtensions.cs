using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.AuditTrail.Business.Configuration;
using Bfs.Iop.AuditTrail.Business.Services;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.DataAccess.Relational;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.AuditTrail.Business;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBusinessServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        services
            .AddSingleton<IValidateOptions<GitOptions>, GitOptionsValidator>()
            .AddOptionsWithValidateOnStart<GitOptions>()
            .Bind(configuration.GetSection(nameof(GitOptions)));

        services.AddScoped<IGitWrapper, GitWrapper>();
        services.AddScoped<IResourceTrackerService, GitResourceTrackerService>();

        // Services need for processing the commits:
        services.AddScoped<IGitCommitProcessorService, GitCommitProcessorService>();
        services.AddScoped<IResourceDataReaderService, ResourceDataReaderService>();
        services.AddSingleton<IMessageQueue<CommitRequest>, ChannelMessageQueue<CommitRequest>>();
        services.AddHostedService<GitCommitBackgroundService>();

        // Database:
        services.TryAddDataAccessServices(options => options
            .UseNpgsql(
                configuration.GetSection("postgresCredentialsSectionKey").Value,
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
            .EnableSensitiveDataLogging()
            .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration);

        return services;
    }
}
