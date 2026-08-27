using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.Core.Settings;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Bfs.Iop.Search.Abstractions;
using Bfs.Iop.Search.Elasticsearch;

namespace Bfs.Iop.IndexSearch.Api;

/// <summary>
/// Every service this host owns, in one place so that a test can build the same container the host
/// builds.
/// <para>
/// This is not tidying. Three separate registrations went missing when this host stopped calling
/// <c>AddIopCoreServices</c> — <c>ApiSettings</c>, <c>IDatasetsService</c> and the code-list search
/// service's dependencies — and each one compiled, passed the whole unit-test suite, and failed only
/// when someone started the process. <c>AddIopCoreServices</c> used to register them as a side
/// effect, so nothing named them anywhere and nothing could miss them.
/// </para>
/// <para>
/// Keeping the registrations addressable from a test is what turns that class of failure back into a
/// build failure. Add new registrations here rather than inline in <c>Program.cs</c>.
/// </para>
/// </summary>
internal static class IndexSearchServiceRegistration
{
    /// <summary>
    /// Registers everything except the ASP.NET plumbing (controllers, CORS, Swagger, problem
    /// details), which needs a real <see cref="WebApplicationBuilder"/> and cannot fail the way the
    /// bugs above did.
    /// </summary>
    /// <param name="services">The container to register into.</param>
    /// <param name="configuration">Host configuration; supplies the database and object-store settings.</param>
    /// <param name="environmentName">
    /// The environment name. It is not just a label: it is the suffix on the object-store container
    /// names ("dataset-structures-Development"), so passing the wrong one silently reads an empty
    /// container rather than failing.
    /// </param>
    /// <param name="isClientGenerator">
    /// True when the NSwag generator is booting this host at build time purely to read its routes.
    /// It must not reach Elasticsearch, the object store or the security configuration while doing
    /// so, which is why the second half of this method is guarded rather than the whole of it — the
    /// generator still needs the database registration to construct <c>IopDbContext</c>.
    /// </param>
    public static IServiceCollection AddIndexSearchServices(
        this IServiceCollection services,
        IConfiguration configuration,
        string environmentName,
        bool isClientGenerator = false)
    {
        // The database, and read-only views over it — NOT the business layer. This host indexes and
        // searches; it never creates or edits a resource, so it has no use for Core's services,
        // MediatR handlers or validators, and referencing them would make the search service rebuild
        // on every business change.
        //
        // NOTE: deliberately does NOT run IIopDatabaseMigrator. IOP Core owns the schema; two hosts
        // migrating concurrently race on data.__EFMigrationsHistory.
        services.AddIopIndexData(configuration);

        // Both object-store consumers need this: it supplies the environment suffix on the container
        // names ("dataset-structures-Development", "filter-configuration-Development").
        services.AddScoped(_ => new ApiSettings { EnvironmentName = environmentName });

        services.Configure<IndexSearchOptions>(configuration.GetSection(IndexSearchOptions.SectionName));

        if (isClientGenerator)
        {
            return services;
        }

        services.TryAddSecurity(configuration);
        services.AddElasticsearchSearch(configuration);

        // hasStructure comes from the object store, not the database, so the reconciler and the
        // rebuild both need the linked-data services.
        services.AddFileStorage(configuration, environmentName);
        services.AddLinkedDataServices(configuration);

        services.AddSingleton<IIndexEventQueue, IndexEventQueue>();
        services.AddSingleton<IIndexBuildState, IndexBuildState>();
        services.AddSingleton<IIndexRebuildTrigger, IndexRebuildTrigger>();
        services.AddScoped<IIndexReconciler, IndexReconciler>();
        services.AddScoped<ICatalogSearchQueryService, CatalogSearchQueryService>();
        services.AddScoped<ICatalogSearchCountQueryService, CatalogSearchCountQueryService>();

        return services;
    }
}
