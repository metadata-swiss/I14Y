using Bfs.Iop.Admin.Models.Geocat;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Bfs.Iop.Admin.GeocatClient;

public static class GeocatClientServiceCollectionExtensions
{
    public static IServiceCollection AddGeocatClient(this IServiceCollection services, IConfiguration configuration)
    {
        var geocatApiBaseUrl = configuration["ApiBaseUrl"];
        var httpProxy = configuration["Proxy"];

        services.AddTransient<IGeocatIndex>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<HttpClient>();
            if (!string.IsNullOrWhiteSpace(httpProxy))
            {
                var clientHandler = new HttpClientHandler { Proxy = new System.Net.WebProxy { Address = new Uri(httpProxy), UseDefaultCredentials = true } };
                client = new HttpClient(clientHandler);
            }

            var mapper = serviceProvider.GetRequiredService<IMapper>();

            var config = serviceProvider.GetRequiredService<TypeAdapterConfig>();
            config.Scan(typeof(GeocatClientServiceCollectionExtensions).Assembly);

            return new GeocatClient(geocatApiBaseUrl, client, mapper);
        });

        return services;
    }
}