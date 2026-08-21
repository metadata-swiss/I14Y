using System.Net.Http;
using Bfs.Iop.Admin.Lindas.Abstractions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Admin.LindasClient;

public static class LindasClientServiceCollectionExtensions
{
    public static IServiceCollection AddLindasClient(this IServiceCollection services, IConfiguration configuration)
    {
        var queryUrl = configuration["QueryUrl"];
        var ldBaseUrl = configuration["LdBaseUrl"];

        services.AddScoped<ILindasClient>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<HttpClient>();
            return new LindasClient(queryUrl, ldBaseUrl, client);
        });

        return services;
    }
}