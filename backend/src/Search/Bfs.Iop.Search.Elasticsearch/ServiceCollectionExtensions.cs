using Bfs.Iop.Search.Elasticsearch.CodeList;
using Bfs.Iop.Search.Abstractions;

using Elastic.Clients.Elasticsearch;
using Elastic.Transport;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Search.Elasticsearch;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Elasticsearch-backed search — the solution's only search engine — covering
    /// both the catalog index and the codelist-entry index.
    /// <para>
    /// Call this from <c>Bfs.Iop.IndexSearch.Api</c> and nowhere else. Every other service reaches
    /// the index over HTTP, which is what keeps the engine out of their dependency graphs.
    /// </para>
    /// </summary>
    public static IServiceCollection AddElasticsearchSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ElasticsearchOptions>()
                .Bind(configuration.GetSection(ElasticsearchOptions.SectionName));

        services.AddSingleton(CreateClient);

        // Scoped, not singleton: these consume IUserContextService, which is scoped. A singleton
        // holding it is a captive dependency the default container rejects outright. Anything
        // resolving these outside a request (the index builder, the event worker) must therefore
        // create its own scope.
        services.AddScoped<ICatalogIndexService, ElasticsearchCatalogIndexService>();

        services.AddScoped<IIndexBuilderService, ElasticsearchCatalogIndexBuilderService>();

        // CodeList-entry index + search. Scoped for the same reason as the catalog index above.
        services.AddScoped<ICodeListEntryIndexService, ElasticsearchCodeListEntryIndexService>();
        services.AddScoped<ICodeListEntrySearchService, ElasticsearchCodeListEntrySearchService>();


        return services;
    }

    private static ElasticsearchClient CreateClient(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<Microsoft.Extensions.Options.IOptions<ElasticsearchOptions>>().Value;

        var settings = new ElasticsearchClientSettings(new Uri(options.Uri));

        if (!string.IsNullOrWhiteSpace(options.Username))
        {
            settings = settings.Authentication(new BasicAuthentication(options.Username, options.Password ?? string.Empty));
        }

        return new ElasticsearchClient(settings);
    }
}
