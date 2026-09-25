using Bfs.Iop.Common.Options;
using Bfs.Iop.Common.Settings;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.FilterConfigurations;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Serialization.Rdf;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddIopCoreServices(
        this IServiceCollection services, 
        IConfiguration configuration,
        string webHostEnvironmentName,
        string webApiClientEnvironmentName)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        var assemblies = new[] { typeof(ServiceCollectionExtensions).Assembly };

        services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(assemblies))
            .TryAddDataAccessServices(options => options
                .UseNpgsql(
                    configuration.GetPostgresDatabaseConnectionString(),
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration)
            .AddIopCoreServices()
            .AddAuditTrailServices(configuration)
            .AddScoped(_ => new ApiSettings() { EnvironmentName = webHostEnvironmentName });

        services
            .AddOptions<I14YOptions>()
            .Bind(configuration.GetSection(I14YOptions.SectionName));

        Console.WriteLine($"EnvironmentName: {webHostEnvironmentName}");
        Console.WriteLine($"WebApiClientEnvironmentName: {webApiClientEnvironmentName}");

        if (webHostEnvironmentName != webApiClientEnvironmentName)
        {
            services
                .AddLinkedDataAndFileStorageServices(configuration);
        }
        
        return services;
    }

    private static IServiceCollection AddIopCoreServices(this IServiceCollection services)
    {
        services
            .AddScoped<IMediaService, MediaService>()
            .AddScoped<IRelationsCountService, RelationsCountService>();

        // RDF serialization
        services.AddScoped<IAgentRdfSerializer, AgentRdfSerializer>();

        return services;
    }

    private static IServiceCollection AddLinkedDataAndFileStorageServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddScoped<FilterConfigurationFileStorageService>()
            .TryAddFileStorage(configuration)
            .AddLinkedDataServices(configuration);
    }

    private static IServiceCollection AddAuditTrailServices(this IServiceCollection services, IConfiguration configuration)
    {
        return services.AddScoped<IAuditTrailNotifierService, PlaceHolderAuditTrailService>();

        // Todo: uncoment once the container can be implemented in azure
        //return services
        //    .AddAuditTrailApiClient(configuration)
        //    .AddSingleton<IMessageQueue<AuditTrailMessage>, ChannelMessageQueue<AuditTrailMessage>>()
        //    .AddScoped<IAuditTrailNotifierService, AuditTrailNotifierService>()
        //    .AddHostedService<AuditTrailDispatcherService>();
    }
}