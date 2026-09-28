
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
