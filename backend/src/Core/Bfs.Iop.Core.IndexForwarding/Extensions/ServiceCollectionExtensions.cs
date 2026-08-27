using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.IndexSearch.ApiClient.Extensions;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.IndexForwarding.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Binds IOP Core's search ports to the standalone IndexSearch service: writes are enqueued and
    /// forwarded, reads are proxied over HTTP. Core keeps no index of its own.
    /// <para>
    /// Both directions are registered together on purpose. They used to be separable because Core had
    /// a local index to fall back on; it no longer does, so a half-configured host would serve
    /// searches from one engine while writing to another, or fail to start business services at all.
    /// </para>
    /// <para>
    /// Requires an <c>IIndexSearchTokenProvider</c> from the composition root — reads are user-scoped,
    /// and dropping the caller's token silently narrows results to public-only with HTTP 200.
    /// </para>
    /// </summary>
    /// <param name="services"></param>
    /// <param name="apiBaseAddress">IndexSearch base URL. Required.</param>
    /// <param name="secret">Pre-shared secret for the write path; must match the service's IndexSearch:Secret.</param>
    /// <exception cref="InvalidOperationException">
    /// The base URL is missing or still an unsubstituted deployment token.
    /// </exception>
    public static IServiceCollection AddIndexSearchIntegration(
        this IServiceCollection services,
        string? apiBaseAddress,
        string? secret)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Fail fast, and fail HERE. This used to degrade quietly to an in-process index, which
        // was harmless. It no longer is: every Core business service takes ICatalogIndexWriter in its
        // constructor, so skipping these registrations makes IDatasetsService, IIopConceptsService,
        // IMappingTablesService, IPublicServicesService and IDataServicesService unresolvable — Core
        // would boot and then 500 on nearly every endpoint, from one missing setting.
        if (!IndexSearchConfiguration.IsConfigured(apiBaseAddress))
        {
            throw new InvalidOperationException(
                $"'{IndexSearchConfiguration.BaseUrlKey}' is not configured. IOP Core has no local " +
                "search index: the IndexSearch service is the only path to the index, for both reads " +
                "and writes, so this setting is required.");
        }

        // Write path: X-Api-Key, fire-and-forget through a bounded channel.
        services.AddIndexSearchApiClient(apiBaseAddress!, secret ?? string.Empty);
        services.AddSingleton<IIndexSearchDispatcher, IndexSearchDispatcher>();
        services.AddHostedService<IndexForwardingSenderHostedService>();

        // One instance behind both write contracts: they share a queue, and DeIndexAsync is
        // implemented explicitly per contract so the two indexes cannot be confused.
        services.AddScoped<ForwardingIndexWriter>();
        services.AddScoped<ICatalogIndexWriter>(sp => sp.GetRequiredService<ForwardingIndexWriter>());
        services.AddScoped<ICodeListEntryIndexWriter>(sp => sp.GetRequiredService<ForwardingIndexWriter>());

        // Read path: the caller's bearer token, forwarded per request.
        //
        // Core's four search handlers inject IIndexSearchSearchClient directly. There is no port and
        // no adapter in between: Core has one index and one way to reach it, so a second abstraction
        // would only offer a second way to answer the same question — and a facet count answered from
        // somewhere other than the result list it labels is the failure this avoids.
        services.AddIndexSearchSearchClient(apiBaseAddress!);

        return services;
    }
}
