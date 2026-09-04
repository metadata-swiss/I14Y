using Bfs.Iop.Common.Options;
using Bfs.Iop.Common.Settings;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.FilterConfigurations;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.Core.Serialization.Rdf;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.DataAccess.Relational;
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
                    GetPostgresDatabaseConnectionString(configuration),
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)), configuration)
            .AddIopCoreServices()
            .AddScoped(_ => new ApiSettings() { EnvironmentName = webHostEnvironmentName });

        services
            .AddOptions<I14YOptions>()
            .Bind(configuration.GetSection(I14YOptions.SectionName));

        Console.WriteLine($"EnvironmentName: {webHostEnvironmentName}");
        Console.WriteLine($"WebApiClientEnvironmentName: {webApiClientEnvironmentName}");

        if (webHostEnvironmentName != webApiClientEnvironmentName)
        {
            services
                .AddLinkedDataAndFileStorageServices(configuration, webHostEnvironmentName);
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
        IConfiguration configuration, 
        string webHostEnvironmentName)
    {
        return services
            .AddScoped<FilterConfigurationFileStorageService>()
            .AddFileStorage(configuration, webHostEnvironmentName)
            .AddLinkedDataServices(configuration);
    }

    private static string GetPostgresDatabaseConnectionString(IConfiguration configuration)
    {
        const string postgresCredentialsSectionKey = "postgresCredentialsSectionKey";
        const string databaseNameKey = "database";
        const string hostnameKey = "hostname";
        const string usernameKey = "username";
        const string passwordKey = "password";
        const string portKey = "port";
        const string poolingOptionsKey = "poolingOptions";

        var section = configuration.GetValue<string>(postgresCredentialsSectionKey) ??
            throw new ConfigurationErrorsException("Postgres credentials section key is not configured.");

        var database = configuration[$"{section}:{databaseNameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{databaseNameKey}'", paramName: nameof(configuration));

        var host = configuration[$"{section}:{hostnameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{hostnameKey}'", paramName: nameof(configuration));

        var password = configuration[$"{section}:{passwordKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{passwordKey}'", paramName: nameof(configuration));

        var port = configuration[$"{section}:{portKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{portKey}'", paramName: nameof(configuration));

        var username = configuration[$"{section}:{usernameKey}"]
            ?? throw new ArgumentException($"Missing configuration value: '{usernameKey}'", paramName: nameof(configuration));

        var baseConnectionString = $"Host={host};Port={port};Username={username};Password={password};Database={database}";

        var poolingOptions = configuration[$"{section}:{poolingOptionsKey}"];

        return string.IsNullOrWhiteSpace(poolingOptions)
            ? baseConnectionString 
            : $"{baseConnectionString};{poolingOptions}";

    }
}