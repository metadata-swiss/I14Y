using Bfs.Iop.Infrastructure.ApiClient.Generator;
using Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api.ClientGenerator;

internal static class Program
{
    private const string Environment = Startup.ClientGeneratorEnvironmentName;

    private static async Task Main(string[] _)
    {
        static TypeScriptClientGeneratorOptions CreateOptions()
        {
            return new TypeScriptClientGeneratorOptions
            {
                ClassName = "BfsIopAdminApiClient",
                OutputPath = "../../../build/ts-client",
                StoreSwaggerJson = false,
                SwaggerJsonUrl = "/swagger/v1/swagger.json",
                ExtensionPath = "TypescriptClientExtensions.ts",
                ConfigurationClass = "BfsIopAdminApiClientSupport",
                BaseUrlTokenName = "IOP_ADMIN_API_BASE_URL",
                WrapResponses = true
            };
        }

        Func<IHostBuilder> hostBuilderFactory = () => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(builder => builder
                .UseStartup<Startup>()
                .UseEnvironment(Environment)
                .ConfigureAppConfiguration((ctx, cb) => cb
                    .AddJsonFile("appsettings.json", false)
                    .AddJsonFile($"appsettings.{Environment}.json", true)
                    .AddEnvironmentVariables()
                )
            ).UseSerilog((context, services, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName);
                    configuration.WriteTo.Console();
            });

        await StaticClientGenerator<Startup>.GenerateTypeScriptClient(CreateOptions(), hostBuilderFactory);
    }
}