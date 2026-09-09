using Azure.Identity;
using Bfs.Iop.Core.ApiClient.Extensions;
using Bfs.Iop.Core.ApiClient.Health;
using Bfs.Iop.Iri.Api.Abstractions.Models;
using Bfs.Iop.Iri.Api.Middleware;
using HealthChecks.UI.Client;
using Microsoft.Extensions.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .ConfigureKestrel(options => options.AddServerHeader = false)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        if (!env.IsDevelopment()) //this is only relevant for Azure, possible environments DEV, ABN, PRD
        {
            var environment = env.EnvironmentName.ToLowerInvariant();
            // attach Azure services, build what we have so far (appsettings.*, env vars, secrets.json, etc.)
            config.Build();

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

            config.Build();
        }
    });

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<I14YOptions>(builder.Configuration.GetSection("I14y"));

builder.Services.AddIopCoreApiClient(builder.Configuration.GetValue<string>("I14Y:IopCoreApiUrl")
    ?? throw new NullReferenceException("I14Y:IopCoreApiUrl"));


builder.Services.AddHealthChecks();

builder.Services.AddHealthChecks()
            .AddCheck<IopCoreApiClientHealthCheck>("Iop Core");

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
    {
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
    };
});

var app = builder.Build();

app.UseMiddleware<HttpTrafficLogger>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseExceptionHandler();

app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    Predicate = (HealthCheckRegistration _) => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapControllers();

app.Run();
