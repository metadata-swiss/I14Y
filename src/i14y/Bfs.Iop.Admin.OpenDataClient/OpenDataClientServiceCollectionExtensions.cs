using AutoMapper;
using Bfs.Iop.Admin.Models.OpenData;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;

namespace Bfs.Iop.Admin.OpenDataClient;

public static class OpenDataClientServiceCollectionExtensions
{
    public static IServiceCollection AddOpenDataClient(this IServiceCollection services, IConfiguration configuration)
    {
        var apiBaseUrl = configuration["ApiBaseUrl"];
        var linkBaseUrl = configuration["LinkBaseUrl"];
        var httpProxy = configuration["Proxy"];

        services.AddTransient<IOpenDataIndex>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<HttpClient>();
            if (!string.IsNullOrWhiteSpace(httpProxy))
            {
                var clientHandler = new HttpClientHandler { Proxy = new System.Net.WebProxy { Address = new Uri(httpProxy), UseDefaultCredentials = true } };
                client = new HttpClient(clientHandler);
            }

            var mapper = serviceProvider.GetRequiredService<IMapper>();

            return new OpenDataClient(apiBaseUrl, linkBaseUrl, client, mapper);
        });
        services.AddSingleton<Profile, OpenDataClientMappingProfile>();
        return services;
    }
}