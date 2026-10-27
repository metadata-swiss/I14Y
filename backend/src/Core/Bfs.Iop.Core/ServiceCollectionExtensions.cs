using Bfs.Iop.AuditTrail.ApiClient;
using Bfs.Iop.Common.Messaging;
using Bfs.Iop.Common.Options;
using Bfs.Iop.Common.Settings;
using Bfs.Iop.Core.CommandHandlers.PublishableTypes;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.FilterConfigurations;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.Core.Messaging.AuditTrail;
using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.Core.Search;
using Bfs.Iop.IndexSearch.ApiClient.Extensions;
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
using System.Configuration;

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
                .AddLinkedDataAndFileStorageServices(configuration)
                .AddIndexSearchServices(configuration);
        }

        return services;
    }

    private static IServiceCollection AddIopCoreServices(this IServiceCollection services)
    {
        services
            .AddScoped<IMediaService, MediaService>()
            .AddScoped<IRelationsCountService, RelationsCountService>()
            .AddScoped<IPublishableResourceNotifier, PublishableResourceNotifier>();

        // RDF serialization
        services.AddScoped<IAgentRdfSerializer, AgentRdfSerializer>();

        return services;
    }

    private static IServiceCollection AddIndexSearchServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var key = $"{I14YOptions.SectionName}:{nameof(I14YOptions.IndexSearchUrl)}";

        var baseUrl = configuration.GetValue<string>(key);

        if (string.IsNullOrWhiteSpace(baseUrl) || baseUrl.Contains("#{", StringComparison.Ordinal))
        {
            var cause = string.IsNullOrWhiteSpace(baseUrl)
                ? "no value"
                : $"'{baseUrl}', which is the release token appsettings.json ships and nothing replaced";

            throw new ConfigurationErrorsException(
                $"'{key}' resolved to {cause}. Core answers every catalog and code-list search from "
                + "the IndexSearch service and has no local engine to fall back to, so it will not "
                + "start without it. Add the key to this environment's App Configuration store under "
                + "the label and prefix in CONTAINER_APP_NAME, pointing at the IndexSearch service in "
                + "the same environment.");
        }

        return services
            .AddScoped<ICodeListEntryIndexSearch, CodeListEntryIndexSearch>()
            .AddIndexSearchApiClient(baseUrl)
            .AddSingleton<IMessageQueue<SearchIndexMessage>, ChannelMessageQueue<SearchIndexMessage>>()
            .AddScoped<ISearchIndexNotifierService, SearchIndexNotifierService>()
            .AddHostedService<SearchIndexDispatcherService>();
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

    private static IServiceCollection AddAuditTrailServices(this IServiceCollection services, IConfiguration configuration) => 
        services
            .AddAuditTrailApiClient(configuration)
            .AddSingleton<IMessageQueue<AuditTrailMessage>, ChannelMessageQueue<AuditTrailMessage>>()
            .AddScoped<IAuditTrailNotifierService, AuditTrailNotifierService>()
            .AddScoped<IResourceStringReaderService, ResourceStringReaderService>()
            .AddHostedService<AuditTrailDispatcherService>();
}