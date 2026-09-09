using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.IndexSearch.Business;
using Bfs.Iop.IndexSearch.Business.Sources;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Data.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection AddIndexSearchDataServices(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        return services
            .AddScoped<ICatalogDocumentSource, CatalogDocumentSource>()
            .AddScoped<ICodeListDocumentSource, CodeListDocumentSource>()

            .AddScoped<IDatasetStructureSource>(provider =>
            {
                var linkedData = provider.GetService<IDatasetModelProcessService>();

                return linkedData is null
                    ? new UnavailableDatasetStructureSource(
                        provider.GetRequiredService<ILogger<UnavailableDatasetStructureSource>>())
                    : new DatasetStructureSource(
                        linkedData,
                        provider.GetRequiredService<ILogger<DatasetStructureSource>>());
            })
            .AddScoped<CatalogIndexRebuilder>()
            .AddScoped<CodeListIndexRebuilder>();
    }
}
