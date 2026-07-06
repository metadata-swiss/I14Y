using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.IO;

namespace Bfs.Iop.Admin.Api.Swagger;

internal class SwaggerInformation
{
    public static readonly string V1 = "v1";

    public SwaggerInformation(string title, string? description = null, string? version = null, string? routePrefix = null, List<SwaggerEndpoint>? swaggerEndpoints = null)
    {
        Title = title;
        Description = description;
        Version = version;
        RoutePrefix = routePrefix;
        SwaggerEndpoints = swaggerEndpoints ?? new List<SwaggerEndpoint>();
    }

    public string? Description { get; }

    public string Title { get; }

    public string? Version { get; }

    public string? RoutePrefix { get; }
    public List<SwaggerEndpoint> SwaggerEndpoints { get; }

    public bool GenerateSwaggerDoc => !string.IsNullOrWhiteSpace(Version);

    public static List<SwaggerInformation> GetSwaggerEnvironments(IConfiguration configuration, IWebHostEnvironment environment)
    {
        return new()
        {
            new SwaggerInformation(
                $"IOP Admin ({environment.EnvironmentName})",
                $"Deployment info: {configuration.GetValue<string>("APP_VERSION") ?? string.Empty}, Assembly: {typeof(Startup).Assembly.GetName().Version?.ToString()}",
                V1,
                "api",
                new List<SwaggerEndpoint>() {new($"/swagger/{V1}/swagger.json", $"IOP Admin ({environment.EnvironmentName}) {V1}") }),

            new SwaggerInformation(
                $"IOP Admin Partner OpenApi File Handler ({environment.EnvironmentName})",
                $"Deployment info: {configuration.GetValue<string>("APP_VERSION") ?? string.Empty}, Assembly: {typeof(Startup).Assembly.GetName().Version?.ToString()}",
                "partner",
                "console/partner-admin",
                new List<SwaggerEndpoint>() {new($"/swagger/partner/swagger.json", $"{environment.EnvironmentName} v1") }),

            new SwaggerInformation(
                $"I14Y Partner API ({environment.EnvironmentName})",
                null,
                null,
                "console/partner/v1",
                GetPartnerApiEndpoints(configuration, environment))
        };
    }

    private static List<SwaggerEndpoint> GetPartnerApiEndpoints(IConfiguration configuration, IWebHostEnvironment environment)
    {
        const string routePrefix = "/console/partner/v1/";

        var swaggerEndpoints = new List<SwaggerEndpoint>();

        var runtimeEnvironment = configuration["ASPNETCORE_ENVIRONMENT"] ?? string.Empty;

        var openApiFilePath = Path.Combine(routePrefix, $"openapi-{runtimeEnvironment.ToLower()}.json");

        var physicalPathOpenApiFile = Path.Combine(environment.WebRootPath, openApiFilePath.TrimStart('/'));

        if (File.Exists(physicalPathOpenApiFile))
        {
            swaggerEndpoints.Add(new SwaggerEndpoint(openApiFilePath, $"{runtimeEnvironment}"));

            var possibleEnvironments = new List<string>()
                    {
                        "QA",
                        "DEV",
                        "TST",
                        "REF",
                        "ABN",
                        "PRD",
                    };

            possibleEnvironments.Remove(runtimeEnvironment); // prevent duplicate SwaggerEndpoint

            if (runtimeEnvironment != "PRD" && runtimeEnvironment != "ABN")
            {
                foreach (var possibleEnvironment in possibleEnvironments)
                {
                    var additionalOpenApiFile = Path.Combine(routePrefix, $"openapi-{possibleEnvironment.ToLower()}.json");

                    var physicalPathAdditionalOpenApiFile = Path.Combine(environment.WebRootPath, additionalOpenApiFile.TrimStart('/'));

                    if (File.Exists(physicalPathAdditionalOpenApiFile))
                    {
                        swaggerEndpoints.Add(new SwaggerEndpoint(additionalOpenApiFile, $"{possibleEnvironment}"));
                    }
                }
            }
        }

        return swaggerEndpoints;
    }
}