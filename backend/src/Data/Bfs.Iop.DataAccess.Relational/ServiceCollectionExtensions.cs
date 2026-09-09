using Bfs.Iop.Core.Validation.Services;
using Bfs.Iop.DataAccess.Authorization;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Authorization;
using Bfs.Iop.DataAccess.Relational.Services;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Relational.Validation.Services;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.DataAccess.Relational;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddDataAccessServices(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptionsBuilderDelegate,
        IConfiguration configuration,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(dbContextOptionsBuilderDelegate, nameof(dbContextOptionsBuilderDelegate));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        if (services.FirstOrDefault(x => x.ServiceType == typeof(IopDbContext)) is not null)
        {
            return services;
        }

        services.AddDbContext<IopDbContext>(dbContextOptionsBuilderDelegate, serviceLifetime, serviceLifetime);
        services.Add(new ServiceDescriptor(typeof(IIopDatabaseMigrator), typeof(IopDatabaseMigrator), serviceLifetime));
        services.AddTransient<DbContext>(sp => sp.GetRequiredService<IopDbContext>());

        // Data services
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
           .AddScoped<IResourceRelationsService, ResourceRelationsService>()
           .AddScoped<ISearchIndexProviderService, SearchIndexProviderService>()
           .AddScoped<IVocabulariesService, VocabulariesService>();

        // Helpers
        services.AddScoped<IIdentifierGenerator, IdentifierGenerator>();

        // Models validation
        services.AddModelsValidation();

        return services;
    }

    private static IServiceCollection AddModelsValidation(this IServiceCollection services)
    {
        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        services
            .AddValidatorsFromAssemblies([assembly], includeInternalTypes: true)
            .AddScoped<IIopConceptsValidationService, IopConceptsValidationService>();

        // The auto discovery from FluentValidation cannot find generic validators, so they must be registered manually.
        return services
            .AddScoped(typeof(VocabularyEntryCodeValidator<>));
    }
}
