using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Api.Config;

internal static class CorsConfig
{
    public const string PolicyName = "AllowBIT";

    internal static void ConfigureCors(this IServiceCollection services, IConfiguration configuration)
    {
        var origins = new List<string>();
        var corsOrigins = configuration.GetSection("CorsOrigins");

        var urls = corsOrigins.GetValue<string>("Urls");
        if (!string.IsNullOrEmpty(urls))
            origins.AddRange(urls.Split(",", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries));

        services.AddCors(options =>
        {
            options.AddPolicy(name: PolicyName,
                builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials()
                        .WithOrigins(origins.ToArray())
                        .WithExposedHeaders("x-paging-pagesize", "x-paging-page", "x-paging-totalpages", "x-paging-totalrows", "location")
                    ;
                });
        });
    }
}