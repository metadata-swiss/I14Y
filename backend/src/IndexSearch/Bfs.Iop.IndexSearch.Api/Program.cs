using System.Text.Json.Serialization;
using Azure.Identity;
using Bfs.Iop.IndexSearch.Api;
using Bfs.Iop.IndexSearch.Api.Health;
using Bfs.Iop.IndexSearch.Api.Indexing;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

// Must run before the DbContext is created: IOP Core sets the same switch in its own host, not
// inside AddIopCoreServices. Without it every DateTimeOffset read from Postgres is shifted.
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

const string ClientGeneratorEnvironmentName = "WebApiClientGenerator";
const string CorsPolicyName = "AllowConfiguredOrigins";

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .ConfigureKestrel(options => options.AddServerHeader = false)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        if (!env.IsDevelopment()) //this is only relevant for Azure, possible environments DEV, ABN, PRD
        {
            var environment = env.EnvironmentName.ToLowerInvariant();
            config.Build();

            var appConfigEndpoint = $"https://bfs-appconfig-i14y-{environment}.azconfig.io";

            var appName = Environment.GetEnvironmentVariable("CONTAINER_APP_NAME"); // set automatically through ACA
            var sharedKey = $"shared-{environment}";

            var azureClientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID"); // this needs to be set manually

            var credentials = azureClientId != null
                ? new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = azureClientId })
                : new DefaultAzureCredential();

            config.AddAzureAppConfiguration(options =>
                options.Connect(new Uri(appConfigEndpoint), credentials)
                       .Select($"{appName}:*", appName)
                       .TrimKeyPrefix($"{appName}:")
                       .Select($"{sharedKey}:*", sharedKey)
                       .TrimKeyPrefix($"{sharedKey}:")
                       .ConfigureKeyVault(kv => kv.SetCredential(credentials)));

            config.Build();
        }
    });

var isClientGenerator = builder.Environment.IsEnvironment(ClientGeneratorEnvironmentName);

// Every service this host owns is registered in one addressable place, so that
// IndexSearchServiceRegistrationTests can build the same container and prove it resolves. Three
// registrations went missing here when this host stopped calling AddIopCoreServices, and each one
// compiled, passed the whole suite, and failed only when someone started the process.
builder.Services.AddIndexSearchServices(
    builder.Configuration,
    builder.Environment.EnvironmentName,
    isClientGenerator);

if (!isClientGenerator)
{
    // Not part of the registration above: a hosted service starts doing real work the moment the
    // container is built, which is exactly what a smoke test must not trigger.
    builder.Services.AddHostedService<IndexEventProcessor>();
    builder.Services.AddHostedService<IndexBuilderHostedService>();

    // "live" answers whether the process is worth keeping alive; "ready" whether it should be given
    // traffic. The Elasticsearch check is both — an unreachable cluster is fatal either way — while
    // an empty index only disqualifies this replica from serving.
    builder.Services.AddHealthChecks()
        .AddCheck<ElasticsearchHealthCheck>("Elasticsearch", tags: ["live", "ready"])
        .AddCheck<IndexReadyHealthCheck>("IndexReady", tags: ["ready"]);
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Must match IOP Core exactly. Without the enum converter, SearchResourceType serialises as
        // an integer and clients break in a way that looks like a data problem.
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddCors(options => options.AddPolicy(CorsPolicyName, policy =>
{
    var origins = builder.Configuration.GetValue<string>("CorsOrigins:Urls")?
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

    policy.WithOrigins(origins)
          .AllowAnyHeader()
          .AllowAnyMethod()
          .AllowCredentials()
          // Paging is returned in headers; without exposing them the browser hides them and the UI
          // silently loses pagination on an otherwise successful 200.
          .WithExposedHeaders(
              HttpContextExtensionsHeaders.Page,
              HttpContextExtensionsHeaders.PageSize,
              HttpContextExtensionsHeaders.TotalPages,
              HttpContextExtensionsHeaders.TotalRows);
}));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddProblemDetails(options =>
{
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler();

app.UseRouting();
app.UseCors(CorsPolicyName);
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Guarded because AddHealthChecks() is: MapHealthChecks resolves HealthCheckService and throws at
// startup when nothing registered it. Mapping unconditionally would make the NSwag generator — which
// boots this host purely to read its routes — fail for a reason unrelated to client generation.
if (!isClientGenerator)
{
    // Liveness. An index that has not been built yet is reported Degraded, which is still a 200, so
    // the platform does not restart the container while it is doing exactly what it should.
    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("live"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });

    // Readiness. Returns 503 while the indexes are empty or being rebuilt, so a replica mid-rebuild
    // is taken out of rotation instead of answering searches with zero results.
    //
    // Inert until the Container App's readiness probe is pointed at this path — the deploy workflows
    // push an image and nothing else, and ACA's default probe is a TCP check on the ingress port.
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
    });
}

app.Run();

/// <summary>Header names used for paging, mirrored from Bfs.Iop.Core.Common.</summary>
internal static class HttpContextExtensionsHeaders
{
    public const string Page = "x-paging-page";
    public const string PageSize = "x-paging-pagesize";
    public const string TotalPages = "x-paging-totalpages";
    public const string TotalRows = "x-paging-totalrows";
}
