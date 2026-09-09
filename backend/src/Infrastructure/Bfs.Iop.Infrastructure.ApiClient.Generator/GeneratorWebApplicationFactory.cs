using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Hosting;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator;

internal class GeneratorWebApplicationFactory<TStartup> : IDisposable where TStartup : class
{
    private readonly IHost _host;
    private readonly HttpClient _client;

    public GeneratorWebApplicationFactory(Func<IHostBuilder> hostBuilderFactory)
    {
        _host = hostBuilderFactory()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>();
                webBuilder.UseTestServer();
            })
            .Start();

        _client = _host.GetTestClient();
    }

    public HttpClient CreateClient() => _client;

    public void Dispose()
    {
        _client.Dispose();
        _host.Dispose();
    }
}