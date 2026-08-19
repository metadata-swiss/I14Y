using Bfs.Iop.Core.Authorization;
using Bfs.Iop.Core.Authorization.Contracts;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.FilterConfigurations;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.Core.Lucene;
using Bfs.Iop.Core.Serialization.Rdf;
using Bfs.Iop.Core.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Validation;
using Bfs.Iop.Core.Validation.Services;
using Bfs.Iop.Core.Validation.Vocabularies;
using FluentValidation;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Reflection;

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
            .TryAddIopDbContext(options => options
                .UseNpgsql(
                    GetPostgresDatabaseConnectionString(configuration),
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
                .EnableSensitiveDataLogging()
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)))
            .AddIopCoreDataServices()
            .AddValidation(assemblies)
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

            // Elasticsearch is no longer hosted in-process: it is owned by the standalone
            // IndexSearch service (src/Search + src/IndexSearch). Core keeps its in-process Lucene
            // index, and when Search:Engine=Elasticsearch the composition root additionally wraps
            // the index services so writes are forwarded to that service.
            services.AddLuceneSearch();
        }

        return services;
    }

    private static IServiceCollection AddIopCoreDataServices(this IServiceCollection services)
    {
        services
            .AddScoped<IRegistrationStatusPolicyService, RegistrationStatusPolicyService>()
            .AddScoped<IPublicationLevelPolicyService, PublicationLevelPolicyService>()
            .AddScoped<IAgentsService, AgentsService>()
            .AddScoped<IDatasetsService, DatasetsService>()
            .AddScoped<IDataServicesService, DataServicesService>()
            .AddScoped<IDcatCatalogRecordsService, DcatCatalogRecordsService>()
            .AddScoped<IDcatCatalogsService, DcatCatalogsService>()
            .AddScoped<IEntityAuthorizationService, EntityAuthorizationService>()
            .AddScoped<IIopConceptsService, IopConceptsService>()
            .AddScoped<IIopPersonsService, IopPersonsService>()
            .AddScoped<IMappingTablesService, MappingTablesService>()
            .AddScoped<IPublicServicesService, PublicServicesService>()
            .AddScoped<IPublishableEntityAuthorizationService, PublishableEntityAuthorizationService>()
            .AddScoped<IVocabulariesService, VocabulariesService>()
            .AddScoped<IMediaService, MediaService>()
            .AddScoped<IRelationsCountService, RelationsCountService>();


        // Add helpers
        services.AddScoped<IIdentifierGenerator, IdentifierGenerator>();

        // RDF serialization
        services.AddScoped<IAgentRdfSerializer, AgentRdfSerializer>();

        return services;
    }

    private static IServiceCollection AddValidation(this IServiceCollection services, IEnumerable<Assembly> assemblies)
    {
        services
            .AddValidatorsFromAssemblies(assemblies, includeInternalTypes: true)
            .AddScoped(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>))
            .AddScoped<IIopConceptsValidationService, IopConceptsValidationService>();

        // The auto discovery from FluentValidation cannot find generic validators, so they must be registered manually.
        return services
            .AddScoped(typeof(VocabularyEntryCodeValidator<>));
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