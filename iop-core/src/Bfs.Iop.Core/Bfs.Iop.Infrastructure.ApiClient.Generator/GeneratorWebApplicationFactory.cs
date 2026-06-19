using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Infrastructure.ApiClient.Generator;

internal class GeneratorWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    private readonly Func<IHostBuilder> _hostBuilderFactory;

    /// <summary>
    /// <inheritdoc cref="GeneratorWebApplicationFactory{TStartup}"/>
    /// </summary>
    /// <param name="hostBuilderFactory">A factory function for the <see cref="IHostBuilder"/>. This should be mostly the same as in your Program.cs file.</param>
    public GeneratorWebApplicationFactory(Func<IHostBuilder> hostBuilderFactory) => 
        _hostBuilderFactory = hostBuilderFactory 
            ?? throw new ArgumentNullException(nameof(hostBuilderFactory));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            services.AddLogging(builder =>
            {
                builder.AddDebug();
                builder.AddConsole();
            });
        });

        base.ConfigureWebHost(builder);
    }

    protected override IHostBuilder CreateHostBuilder() => 
        _hostBuilderFactory.Invoke();
}
