using Azure.Identity;
using Bfs.Iop.Core.Data.Contracts;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Bfs.Iop.Core.Api;

/// <summary>
/// The program to create and start the host application.
/// </summary>
public static class Program
{
    /// <summary>
    /// Setup the host builder.
    /// </summary>
    /// <param name="args"></param>
    /// <returns></returns>
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .UseLamar()
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder.ConfigureKestrel(options => options.AddServerHeader = false)
                          .UseStartup<Startup>()
            )
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Trace);
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;

                if (!env.IsDevelopment()) //this is only relevant for Azure, possible environments DEV, ABN, PRD
                {
                    var environment = env.EnvironmentName.ToLowerInvariant();
                    // attach Azure services, build what we have so far (appsettings.*, env vars, secrets.json, etc.)
                    IConfiguration built = config.Build();

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
                    built = config.Build();

                    var keyVaultUri = built["Azure:KeyVault:Uri"];
                    if (!string.IsNullOrWhiteSpace(keyVaultUri))
                    {
                        config.AddAzureKeyVault(new Uri(keyVaultUri), credentials);
                    }
                }
            });

    /// <summary>
    /// The program entry point.
    /// </summary>
    /// <param name="args"></param>
    public static async Task Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();

        AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

        using (var scope = host.Services.CreateScope())
        {
            var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();

            var migrator = scope.ServiceProvider.GetRequiredService<IIopDatabaseMigrator>();
            await migrator.MigrateAsync(cancellationToken: default);

            if (env.IsDevelopment() || env.IsEnvironment("QA") || env.IsEnvironment("DEV") || env.IsEnvironment("REF"))
            {
                await migrator.InsertSamplesAsync(cancellationToken: default);
            }
        }
        host.Run();
    }
}