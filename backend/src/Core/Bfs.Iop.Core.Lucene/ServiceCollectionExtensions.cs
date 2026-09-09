using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Lucene.IndexBuilders;
using Bfs.Iop.Core.Lucene.Search;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.Lucene;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLuceneSearch(this IServiceCollection services)
    {
        services.AddSingleton<ICodeListEntryIndexService, CodeListEntryIndexService>()
                .AddSingleton<ICatalogIndexService, CatalogIndexService>();

        services.AddIndexBuilders();

        services.AddScoped<ICodeListEntrySearchService, CodeListEntrySearchService>();
        services.AddScoped<LuceneIndexBuilderService>();
        services.AddHostedService<LuceneHostedService>();

        return services;
    }

    private static IServiceCollection AddIndexBuilders(this IServiceCollection services)
    {
        return services
            .AddScoped<IIndexBuilderService, CatalogIndexBuilderService>();
    }
}
