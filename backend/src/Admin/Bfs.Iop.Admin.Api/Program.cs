using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Formatting.Compact;
using System;
using System.Threading.Tasks;

namespace Bfs.Iop.Admin.Api;

/// <summary>
/// The program to create and start the host application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Setup the host.
    /// </summary>
    /// <param name="args"></param>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration((hostingContext, config) =>
            {
                var env = hostingContext.HostingEnvironment;

                if (!env.IsDevelopment()) //this is only relevant for Azure, possible environments DEV, ABN, PRD
                {
                    var environment = env.EnvironmentName.ToLowerInvariant();

                    var appConfigEndpoint = $"https://bfs-appconfig-i14y-{environment}.azconfig.io";


                    var appName = Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"); // this environment variable is automatically set through ACA
                    var sharedKey = $"shared-{environment}";

                    var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"); // this needs to be set manually

                    var credentials = azureClientId != null ?
                        new DefaultAzureCredential(
                            new DefaultAzureCredentialOptions
                            {
                                ManagedIdentityClientId = azureClientId
                            })
                    :
                        new DefaultAzureCredential();

                    config.AddAzureAppConfiguration(options =>
                        options.Connect(new Uri(appConfigEndpoint), credentials) // this connects to Azure App Configuration
                               .Select($"{appName}:*", appName)
                               .TrimKeyPrefix($"{appName}:")
                               .Select($"{sharedKey}:*", sharedKey)
                               .TrimKeyPrefix($"{sharedKey}:")
                               .ConfigureKeyVault(kv => kv.SetCredential(credentials)));
                }
            })
            .ConfigureWebHostDefaults(webBuilder => webBuilder
                .ConfigureKestrel(o =>
                {
                    o.AddServerHeader = false;
                })
                .UseStartup<Startup>()
            )
            .UseSerilog((context, services, configuration) =>
            {
                configuration.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName);
                if (IsDevelopment(context))
                    configuration.WriteTo.Console();
                else
                    configuration.WriteTo.Console(new RenderedCompactJsonFormatter());
            })
        ;

    /// <summary>
    /// The program entry point.
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        host.Run();
    }

    private static bool IsDevelopment(HostBuilderContext context)
    {
        return context.HostingEnvironment.EnvironmentName == "Development";
    }
}