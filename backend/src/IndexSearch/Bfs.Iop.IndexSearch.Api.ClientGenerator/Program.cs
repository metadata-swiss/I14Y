using Bfs.Iop.IndexSearch.Api;
using Bfs.Iop.Infrastructure.ApiClient.Generator;
using Bfs.Iop.Infrastructure.ApiClient.Generator.Csharp;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace Bfs.Iop.IndexSearch.Api.ClientGenerator;

internal static class Program
{
    private const string Environment = Startup.ClientGeneratorEnvironmentName;

    private static async Task Main(string[] _)
    {
        static CsharpClientGeneratorOptions CreateOptions()
        {
            return new CsharpClientGeneratorOptions
            {
                ClassName = "IndexSearchApiClient",
                ClientNamespace = "Bfs.Iop.IndexSearch.ApiClient",
                GenerateClientInterfaces = true,
                OutputPath = "../Bfs.Iop.IndexSearch.ApiClient/Generated",
                SwaggerJsonUrl = "/swagger/v1/swagger.json",
                GenerateDtoTypes = true
            };
        }

        Console.WriteLine("Starting client generator.");

        Func<IHostBuilder> hostBuilderFactory = () => Host.CreateDefaultBuilder()
            .ConfigureWebHostDefaults(builder =>
            {
                builder
                    .UseStartup<Startup>()
                    .UseContentRoot(Path.GetDirectoryName(typeof(Startup).Assembly.Location)!)
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
