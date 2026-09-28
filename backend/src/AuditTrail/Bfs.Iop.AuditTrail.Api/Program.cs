
using Azure.Identity;
using Bfs.Iop.AuditTrail.Api.Health;
using Bfs.Iop.AuditTrail.Business;
using Bfs.Iop.Infrastructure.Security;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using System.Text.Json.Serialization;

namespace Bfs.Iop.AuditTrail.Api;

public class Program
{
    public static void Main(string[] args)
    {
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

        // Add services to the container.
        builder.Services.AddBusinessServices(builder.Configuration);

        builder.Services.TryAddSecurity(builder.Configuration, builder.Environment.IsDevelopment());

        builder.Services
            .AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
            });

        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        // Swagger UI
        builder.Services.AddSwaggerGen(options =>
        {
            var assemblyVersion = typeof(Program).Assembly.GetName().Version?.ToString() ?? "1";
            var releaseVersion = builder.Configuration.GetValue<string>("APP_VERSION") ?? string.Empty;

            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = $"Audit Trail API ({builder.Environment.EnvironmentName})",
                Version = "v1",
                Description = $"Deployment info: {releaseVersion}, Assembly: {assemblyVersion}"
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT"
            });

            options.AddSecurityRequirement(document => new OpenApiSecurityRequirement
            {
                [new OpenApiSecuritySchemeReference("Bearer", document)] = []
            });
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck<GitHealthCheck>("git");

        var app = builder.Build();

        if (app.Environment.IsDevelopment() || app.Environment.IsEnvironment("DEV"))
        {
            app.MapOpenApi();
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var version = builder.Configuration.GetValue<string>("APP_VERSION") ?? "v1";

                options.DefaultModelsExpandDepth(-1);
                options.SwaggerEndpoint("/swagger/v1/swagger.json", $"Audit trail {version}");
                options.RoutePrefix = "api";
                options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
            });
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.Run();
    }
}
