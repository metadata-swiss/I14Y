using Bfs.Iop.Infrastructure.ApiClient.Generator;
using Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;
using Bfs.Iop.Infrastructure.ApiClient.Generator.TypeScript;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.IO;
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
                OutputPath = "../bfs-iop-admin-ui/projects/bfs-i14y/bfs-iop-admin-web-api-client/src/lib/generated",
                StoreSwaggerJson = false,
                SwaggerJsonUrl = "/swagger/v1/swagger.json",
                ExtensionPath = "TypescriptClientExtensions.ts",
                ConfigurationClass = "BfsIopAdminApiClientSupport",
                BaseUrlTokenName = "IOP_ADMIN_API_BASE_URL",
                WrapResponses = true
            };
        }

        static CsharpClientGeneratorOptions CreateCsharpOptions()
        {
            return new CsharpClientGeneratorOptions
            {
                ClassName = "IopAdminApiClient",
                ClientNamespace = "Bfs.Iop.Admin.Api.ApiClient",
                GenerateClientInterfaces = true,
                OutputPath = "../Bfs.Iop.Admin.ApiClient/Generated",
                SwaggerJsonUrl = $"/swagger/v1/swagger.json",
                GenerateDtoTypes = false,
                AdditionalNamespaceUsages =
                [
                    "Bfs.Iop.Admin.Models",
                    "Bfs.Iop.Core.Abstractions.Models",
                    "Bfs.Iop.Core.Abstractions.Models.LinkedData",
                    "Bfs.Iop.Core.Abstractions.Models.Search",
                    "Bfs.Iop.Core.Abstractions.Models.FilterConfigurations"
                ],
            };
        }

        Console.WriteLine("Starting client generator.");

        Func<IHostBuilder> hostBuilderFactory = () => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(builder => builder
                .UseStartup<Startup>()
                .UseContentRoot(Path.GetDirectoryName(typeof(Startup).Assembly.Location))
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
        await StaticClientGenerator<Startup>.GenerateCSharpClient(CreateCsharpOptions(), hostBuilderFactory);
    }
}