using Azure.Identity;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.IndexSearch.Api;
using Bfs.Iop.IndexSearch.Api.Authorization;
using Bfs.Iop.IndexSearch.Api.Filters;
using Bfs.Iop.IndexSearch.Api.Health;
using Bfs.Iop.IndexSearch.Api.Hosting;
using Bfs.Iop.IndexSearch.Data.Extensions;
using Bfs.Iop.IndexSearch.Elasticsearch.Extensions;
using Bfs.Iop.Infrastructure.Security;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Migrations;
using System.Configuration;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost
    .ConfigureKestrel(options => options.AddServerHeader = false)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        if (!env.IsDevelopment()) 
        {
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
        }
    });

builder.Logging
    .ClearProviders()
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddConsole();

builder.Services
    .AddOptions<IndexSearchOptions>()
    .Bind(builder.Configuration.GetSection(IndexSearchOptions.SectionName));

builder.Services.TryAddDataAccessServices(
    options => options
        .UseNpgsql(
            GetPostgresConnectionString(builder.Configuration),
            x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
        .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)),
    builder.Configuration);

builder.Services
    .AddIndexSearchDataServices()
    .AddIndexSearchElasticsearch(builder.Configuration);

if (!string.IsNullOrWhiteSpace(builder.Configuration["TripleStore:Endpoint"]))
{
    builder.Services.AddLinkedDataServices(builder.Configuration);
}

builder.Services.TryAddSecurity(builder.Configuration, builder.Environment.IsDevelopment());

builder.Services.AddScoped<ISearchCallerFactory, SearchCallerFactory>();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<ReindexGate>();
builder.Services.AddSingleton<ReindexOrchestrator>();
builder.Services.AddScoped<IndexApiKeyFilter>();
builder.Services.AddHostedService<IndexStartupService>();

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services
    .AddHealthChecks()
    .AddCheck<ElasticsearchHealthCheck>(
        "Elasticsearch",
        failureStatus: Microsoft.Extensions.Diagnostics.HealthChecks.HealthStatus.Unhealthy,
        tags: [],
        timeout: TimeSpan.FromSeconds(5));

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

app.UseHttpsRedirection();
app.UseExceptionHandler();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
});

app.MapControllers();

app.Run();

static string GetPostgresConnectionString(IConfiguration configuration)
{
    const string sectionKey = "postgresCredentialsSectionKey";

    var section = configuration.GetValue<string>(sectionKey)
        ?? throw new ConfigurationErrorsException($"'{sectionKey}' is not configured.");

    string Required(string key) => configuration[$"{section}:{key}"]
        ?? throw new ConfigurationErrorsException($"Missing configuration value '{section}:{key}'.");

    var connection =
        $"Host={Required("hostname")};Port={Required("port")};Username={Required("username")};"
        + $"Password={Required("password")};Database={Required("database")}";

    var pooling = configuration[$"{section}:poolingOptions"];

    return string.IsNullOrWhiteSpace(pooling) ? connection : $"{connection};{pooling}";
}
