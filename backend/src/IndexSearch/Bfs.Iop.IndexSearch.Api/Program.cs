using Azure.Identity;

namespace Bfs.Iop.IndexSearch.Api;


public static class Program
{
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
                webBuilder
                    .ConfigureKestrel(options => options.AddServerHeader = false)
                    .UseStartup<Startup>())
            .ConfigureLogging((context, builder) =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(context.Configuration.GetSection("Logging"));
                builder.AddConsole();
            })
            .ConfigureAppConfiguration((context, config) =>
            {
                var env = context.HostingEnvironment;

                if (env.IsDevelopment()
                    || env.EnvironmentName.Equals(
                        Startup.ClientGeneratorEnvironmentName,
                        StringComparison.Ordinal))
                {
                    return;
                }

                var environment = env.EnvironmentName.ToLowerInvariant();
                config.Build();

                var appConfigEndpoint = $"https://bfs-appconfig-i14y-{environment}.azconfig.io";

                var appName = Environment.GetEnvironmentVariable("CONTAINER_APP_NAME");
                var sharedKey = $"shared-{environment}";

                var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");

                var credentials = azureClientId != null
                    ? new DefaultAzureCredential(
                        new DefaultAzureCredentialOptions { ManagedIdentityClientId = azureClientId })
                    : new DefaultAzureCredential();

                config.AddAzureAppConfiguration(options =>
                    options.Connect(new Uri(appConfigEndpoint), credentials)
                           .Select($"{appName}:*", appName)
                           .TrimKeyPrefix($"{appName}:")
                           .Select($"{sharedKey}:*", sharedKey)
                           .TrimKeyPrefix($"{sharedKey}:")
                           .ConfigureKeyVault(kv => kv.SetCredential(credentials)));

                config.Build();
            });

    public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();
}
