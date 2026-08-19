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
    /// Registers the Elasticsearch-backed search (PoC). Alternative to <c>AddLuceneSearch()</c>, covering
    /// both the catalog index and the codelist-entry index.
    /// </summary>
    public static IServiceCollection AddElasticsearchSearch(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddOptions<ElasticsearchOptions>()
                .Bind(configuration.GetSection(ElasticsearchOptions.SectionName));

        services.AddSingleton(CreateClient);

        // Concrete registered as singleton; the interface forwards to it so the hosted service can call
        // EnsureIndexAsync while the command handlers see it through ICatalogIndexService.
        services.AddSingleton<ElasticsearchCatalogIndexService>();
        services.AddSingleton<ICatalogIndexService>(sp => sp.GetRequiredService<ElasticsearchCatalogIndexService>());

        services.AddScoped<IIndexBuilderService, ElasticsearchCatalogIndexBuilderService>();

        // CodeList-entry index + search (Part C). Concrete registered so the hosted service can call
        // EnsureIndexAsync/BuildIndex; the interface forwards to it (IopConceptsService also depends on it
        // for live CRUD).
        services.AddSingleton<ElasticsearchCodeListEntryIndexService>();
        services.AddSingleton<ICodeListEntryIndexService>(sp => sp.GetRequiredService<ElasticsearchCodeListEntryIndexService>());
        services.AddScoped<ICodeListEntrySearchService, ElasticsearchCodeListEntrySearchService>();

        services.AddHostedService<ElasticsearchHostedService>();

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
