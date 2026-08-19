using System.Text.Json.Serialization;
using Azure.Identity;
using Bfs.Iop.Core;
using Bfs.Iop.IndexSearch.Api.Health;
using Bfs.Iop.IndexSearch.Api.Indexing;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Search.Elasticsearch;
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

// NOTE: deliberately does NOT run IIopDatabaseMigrator. IOP Core owns the schema; two hosts
// migrating concurrently race on data.__EFMigrationsHistory.
builder.Services.AddIopCoreServices(
    builder.Configuration,
    builder.Environment.EnvironmentName,
    ClientGeneratorEnvironmentName);

builder.Services.Configure<IndexSearchOptions>(builder.Configuration.GetSection(IndexSearchOptions.SectionName));

if (!isClientGenerator)
{
    builder.Services.TryAddSecurity(builder.Configuration);
    builder.Services.AddElasticsearchSearch(builder.Configuration);

    builder.Services.AddSingleton<IIndexEventQueue, IndexEventQueue>();
    builder.Services.AddSingleton<IIndexBuildState, IndexBuildState>();
    builder.Services.AddScoped<IIndexReconciler, IndexReconciler>();
    builder.Services.AddScoped<ICatalogSearchQueryService, CatalogSearchQueryService>();
    builder.Services.AddHostedService<IndexEventProcessor>();
    builder.Services.AddHostedService<IndexBuilderHostedService>();

    builder.Services.AddHealthChecks()
        .AddCheck<ElasticsearchHealthCheck>("Elasticsearch");
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

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.Run();

/// <summary>Header names used for paging, mirrored from Bfs.Iop.Core.Common.</summary>
internal static class HttpContextExtensionsHeaders
{
    public const string Page = "x-paging-page";
    public const string PageSize = "x-paging-pagesize";
    public const string TotalPages = "x-paging-totalpages";
    public const string TotalRows = "x-paging-totalrows";
}
