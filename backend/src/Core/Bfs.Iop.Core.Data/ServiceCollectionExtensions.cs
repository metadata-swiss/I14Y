using Bfs.Iop.Core.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Bfs.Iop.Core.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddIopDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptionsBuilderDelegate,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(dbContextOptionsBuilderDelegate, nameof(dbContextOptionsBuilderDelegate));

        if (services.FirstOrDefault(x => x.ServiceType == typeof(IopDbContext)) is null)
        {
            services.AddDbContext<IopDbContext>(dbContextOptionsBuilderDelegate, serviceLifetime, serviceLifetime);
            services.Add(new ServiceDescriptor(typeof(IIopDatabaseMigrator), typeof(IopDatabaseMigrator), serviceLifetime));
            services.AddTransient<DbContext>(sp => sp.GetRequiredService<IopDbContext>());
        }

        return services;
    }

    /// <summary>
    /// Registers the read-only view of the search corpus.
    /// <para>
    /// Separate from <see cref="TryAddIopDbContext"/> so a host can take the database without taking
    /// the index reader, and — more to the point — so the IndexSearch service can rebuild the index
    /// with a reference to this project alone, instead of to the whole business layer.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIopIndexDataReader(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        services.AddScoped<Indexing.IIndexDataReader, Indexing.IndexDataReader>();
        services.AddScoped<Indexing.IAgentReader, Indexing.AgentReader>();
        services.AddScoped<Vocabularies.IVocabularyReader, Vocabularies.VocabularyReader>();

        // Code-list search needs both: the guard replaces the concept authorization check that used
        // to come from IIopConceptsService, and the reader replaces a MediatR hop into a handler that
        // is internal to Bfs.Iop.Core. Without them the search service cannot be constructed at all.
        services.AddScoped<Indexing.IConceptAccessGuard, Indexing.ConceptAccessGuard>();
        services.AddScoped<Indexing.IFilterConfigurationReader, Indexing.FilterConfigurationReader>();
        services.AddScoped<Indexing.ICodeListEntryReader, Indexing.CodeListEntryReader>();

        return services;
    }

    /// <summary>
    /// Everything a search host needs from the database: the context plus the read-only views.
    /// <para>
    /// Deliberately does NOT register <c>IIopDatabaseMigrator</c> beyond what
    /// <see cref="TryAddIopDbContext"/> adds, and the caller must not run it — IOP Core owns the
    /// schema, and two hosts migrating concurrently race on <c>data.__EFMigrationsHistory</c>.
    /// </para>
    /// </summary>
    public static IServiceCollection AddIopIndexData(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        return services
            .TryAddIopDbContext(options => options
                .UseNpgsql(
                    GetPostgresConnectionString(configuration),
                    x => x.MigrationsHistoryTable("__EFMigrationsHistory", "data")))
            .AddIopIndexDataReader();
    }

    /// <summary>
    /// Builds the Postgres connection string from the indirect credentials layout this deployment
    /// uses: a <c>postgresCredentialsSectionKey</c> naming the section that holds the parts.
    /// <para>
    /// Public because more than one host needs it and duplicating the key names is how they drift.
    /// </para>
    /// </summary>
    public static string GetPostgresConnectionString(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        const string postgresCredentialsSectionKey = "postgresCredentialsSectionKey";

        var section = configuration.GetValue<string>(postgresCredentialsSectionKey)
            ?? throw new InvalidOperationException("Postgres credentials section key is not configured.");

        string Required(string key) => configuration[$"{section}:{key}"]
            ?? throw new ArgumentException($"Missing configuration value: '{key}'", nameof(configuration));

        var baseConnectionString =
            $"Host={Required("hostname")};Port={Required("port")};Username={Required("username")};" +
            $"Password={Required("password")};Database={Required("database")}";

        var poolingOptions = configuration[$"{section}:poolingOptions"];

        return string.IsNullOrWhiteSpace(poolingOptions)
            ? baseConnectionString
            : $"{baseConnectionString};{poolingOptions}";
    }
}
