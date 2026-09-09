using Bfs.Iop.Core.LinkedData.Configuration;
using Bfs.Iop.Core.LinkedData.Factories;
using Bfs.Iop.Core.LinkedData.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Core.LinkedData;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddLinkedDataServices(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var tripleStoreConfig = new TripleStoreConfiguration();
        configuration.Bind("TripleStore", tripleStoreConfig);

        // if there is connectstring we use tripleStore
        if (!string.IsNullOrWhiteSpace(tripleStoreConfig.Endpoint))
        {
            services.AddSingleton(tripleStoreConfig);
            services.AddSingleton<FusekiConnectionFactory>();
            services.AddScoped<IDatasetModelProcessService, DatasetModelTripleStoreProcessService>();
        }
        else
        {
            services
                .AddScoped<IDatasetModelProcessService, DatasetModelFileProcessService>();
        }

        return services;
    }
}