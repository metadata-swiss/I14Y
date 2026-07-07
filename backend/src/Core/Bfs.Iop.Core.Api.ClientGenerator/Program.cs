using Bfs.Iop.Infrastructure.ApiClient.Generator;
using Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api.ClientGenerator;

internal static class Program
{
    private const string Environment = Startup.ClientGeneratorEnvironmentName;

    private static async Task Main(string[] _)
    {
        static CsharpClientGeneratorOptions CreateOptions()
        {
            return new CsharpClientGeneratorOptions
            {
                ClassName = "IopCoreApiClient",
                ClientNamespace = "Bfs.Iop.Core.ApiClient",
                GenerateClientInterfaces = true,
                OutputPath = "../Bfs.Iop.Core.ApiClient/Generated",
                SwaggerJsonUrl = "/swagger/v1/swagger.json",
                GenerateDtoTypes = false,
                AdditionalNamespaceUsages = [
                    "Bfs.Iop.Core.Abstractions.Models",
                    "Bfs.Iop.Core.Abstractions.Models.Search",
                    "Bfs.Iop.Core.Abstractions.Models.LinkedData",
                    "Bfs.Iop.Core.Abstractions.Models.FilterConfigurations"]
            };
        }

        Console.WriteLine("Starting client generator.");

        Func<IHostBuilder> hostBuilderFactory = () => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .UseStartup<Startup>()
                    .UseContentRoot(Path.GetDirectoryName(typeof(Startup).Assembly.Location))
                    .UseEnvironment(Environment)
                    .ConfigureAppConfiguration((ctx, cb) => cb
                        .AddJsonFile("appsettings.json", false)
                        .AddJsonFile($"appsettings.{Environment}.json", true)
                        .AddEnvironmentVariables()
                    );
            });

        await StaticClientGenerator<Startup>.GenerateCSharpClient(CreateOptions(), hostBuilderFactory);
    }
}