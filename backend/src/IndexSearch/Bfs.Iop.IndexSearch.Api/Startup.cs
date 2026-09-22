using Bfs.Iop.Core.LinkedData;
using Bfs.Iop.DataAccess.Relational;
using Bfs.Iop.IndexSearch.Api.Authorization;
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
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi;
using System.Configuration;
using System.Text.Json.Serialization;

namespace Bfs.Iop.IndexSearch.Api;

public class Startup
{
    public const string ClientGeneratorEnvironmentName = "WebApiClientGenerator";

    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;
    }

    public IConfiguration Configuration { get; }
    public IWebHostEnvironment Environment { get; }

    private bool IsGeneratingClient =>
        Environment.EnvironmentName.Equals(ClientGeneratorEnvironmentName, StringComparison.Ordinal);

    public void ConfigureServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        if (!IsGeneratingClient)
        {
            AddSearchAndIndexing(services);

            services.TryAddSecurity(Configuration, Environment.IsDevelopment());

            services.AddAuthorizationBuilder()
                .AddPolicy(IndexPolicies.Rebuild, IndexPolicies.ConfigureRebuild);

            services.AddScoped<ISearchCallerFactory, SearchCallerFactory>();
        }

        services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        services.AddEndpointsApiExplorer();

        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header. Enter 'Bearer' [space] and then your token.",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = [],
            });
        });

        services.AddHealthChecks();
        services.AddRouting(options => options.LowercaseUrls = true);
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = ctx =>
                ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier;
        });
    }

    public void Configure(IApplicationBuilder app)
    {
        ArgumentNullException.ThrowIfNull(app);

        if (Environment.IsDevelopment() || IsGeneratingClient)
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();
        app.UseExceptionHandler();

        app.UseRouting();

        if (!IsGeneratingClient)
        {
            app.UseAuthentication();
            app.UseAuthorization();
        }

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();

            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => true,
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse,
            });
        });
    }

    private void AddSearchAndIndexing(IServiceCollection services)
    {
        services.TryAddDataAccessServices(
            options => options
                .UseNpgsql(
                    GetPostgresConnectionString(Configuration),
                    x => x.MigrationsHistoryTable(HistoryRepository.DefaultTableName, "data"))
                .ConfigureWarnings(w => w.Ignore(RelationalEventId.PendingModelChangesWarning)),
            Configuration);

        services
            .AddIndexSearchDataServices()
            .AddIndexSearchElasticsearch(Configuration);

        if (!string.IsNullOrWhiteSpace(Configuration["TripleStore:Endpoint"]))
        {
            services.AddLinkedDataServices(Configuration);
        }

        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<ReindexGate>();
        services.AddSingleton<ReindexOrchestrator>();
        services.AddHostedService<IndexStartupService>();

        services
            .AddOptions<ReindexScheduleOptions>()
            .Bind(Configuration.GetSection(ReindexScheduleOptions.SectionName))
            .ValidateOnStart();

        services
            .AddSingleton<IValidateOptions<ReindexScheduleOptions>, ReindexScheduleOptionsValidation>();

        if (!Environment.IsDevelopment())
        {
            services.AddHostedService<ScheduledReindexService>();
        }

        services
            .AddHealthChecks()
            .AddCheck<ElasticsearchHealthCheck>(
                "Elasticsearch",
                failureStatus: HealthStatus.Unhealthy,
                tags: [],
                timeout: TimeSpan.FromSeconds(5));
    }

    private static string GetPostgresConnectionString(IConfiguration configuration)
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
}
