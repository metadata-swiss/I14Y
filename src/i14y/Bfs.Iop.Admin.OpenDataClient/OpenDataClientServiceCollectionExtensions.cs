using Bfs.Iop.Admin.Models.OpenData;
using MapsterMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;
using System.Net.Http;

namespace Bfs.Iop.Admin.OpenDataClient;

public static class OpenDataClientServiceCollectionExtensions
{
    public static IServiceCollection AddOpenDataClient(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"];
        var linkBaseUrl = configuration["LinkBaseUrl"];
        var httpProxy = configuration["Proxy"];

        services.AddTransient<IOpenDataIndex>(serviceProvider =>
        {
            HttpClient client;

            if (!string.IsNullOrWhiteSpace(httpProxy))
            {
                var handler = new HttpClientHandler
                {
                    Proxy = new WebProxy
                    {
                        Address = new Uri(httpProxy),
                        UseDefaultCredentials = true
                    }
                };

                client = new HttpClient(handler);
            }
            else
            {
                client = serviceProvider.GetRequiredService<HttpClient>();
            }

            var mapper = serviceProvider.GetRequiredService<IMapper>();

            return new OpenDataClient(apiBaseUrl, linkBaseUrl, client, mapper);
        });

        return services;
    }
}