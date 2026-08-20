using Bfs.Iop.Admin.Models.Lindas;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;

namespace Bfs.Iop.Admin.LindasClient;

public static class LindasClientServiceCollectionExtensions
{
    public static IServiceCollection AddLindasClient(this IServiceCollection services, IConfiguration configuration)
    {
        var queryUrl = configuration["QueryUrl"];
        var ldBaseUrl = configuration["LdBaseUrl"];

        services.AddTransient<ILindasClient>(serviceProvider =>
        {
            var client = serviceProvider.GetRequiredService<HttpClient>();
            return new LindasClient(queryUrl, ldBaseUrl, client);
        });

        return services;
    }
}