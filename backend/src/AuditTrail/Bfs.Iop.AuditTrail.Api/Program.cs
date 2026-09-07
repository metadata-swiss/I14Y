
using Bfs.Iop.AuditTrail.Api.Health;
using Bfs.Iop.AuditTrail.Business;
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
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Audit Trail API",
                Version = "v1"
            });
        });

        builder.Services
            .AddHealthChecks()
            .AddCheck<GitHealthCheck>("git");

        var app = builder.Build();

        app.MapOpenApi();
        app.UseSwagger();
        app.UseSwaggerUI(options =>
        {
            var version = builder.Configuration.GetValue<string>("APP_VERSION") ?? "v1";

            options.DefaultModelsExpandDepth(-1);
            options.SwaggerEndpoint($"/swagger/{version}/swagger.json", $"Audit trail {version}");
            options.RoutePrefix = "api";
            options.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
        });

        app.Run();
    }
}
