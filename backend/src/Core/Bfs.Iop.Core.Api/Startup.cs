using Bfs.Iop.Core.Api.Filters;
using Bfs.Iop.Core.Api.Health;
using Bfs.Iop.Core.Api.Middleware;
using Bfs.Iop.Core.Api.Swagger;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Api.Exceptions;
using Bfs.Iop.Core.Lucene;
using Bfs.Iop.Infrastructure.Security;
using FluentValidation;
using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Bfs.Iop.Core.Api;

/// <summary>
/// Start up the application.
/// </summary>
public class Startup
{
    /// <summary>
    /// The client generator environment name.
    /// </summary>
    public const string ClientGeneratorEnvironmentName = "WebApiClientGenerator";

    private readonly SwaggerInformation _swaggerInformation;

    /// <summary>
    /// Initializes a <see cref="Startup"/> instance.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="environment"></param>
    public Startup(IConfiguration configuration, IWebHostEnvironment environment)
    {
        Configuration = configuration;
        Environment = environment;

        _swaggerInformation = new SwaggerInformation(environment.EnvironmentName, configuration.GetValue<string>("APP_VERSION") ?? string.Empty);
    }

    /// <inheritdoc cref="IConfiguration" />
    public IConfiguration Configuration { get; }

    /// <inheritdoc cref="IWebHostEnvironment" />
    public IWebHostEnvironment Environment { get; }

    /// <summary>
    /// Configure is where you add middleware. This is called after ConfigureContainer. You can use IApplicationBuilder.ApplicationServices here if you need to resolve things from the container.
    /// </summary>
    public void Configure(IApplicationBuilder app)
    {
        // Use HTTP traffic logger, which allows to log all requests and responses.
        app.UseMiddleware<HttpTrafficLogger>();

        // Apply Exception Mappings.
        app.UseProblemDetails();

        app.UseSwagger();
        app.UseSwaggerUI(c =>
        {
            c.DefaultModelsExpandDepth(-1);
            c.SwaggerEndpoint($"/swagger/{_swaggerInformation.Version}/swagger.json", $"{_swaggerInformation.Title} {_swaggerInformation.Version}");
            c.RoutePrefix = "api";
            c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        });

        app.UseStaticFiles();

        app.UseHttpsRedirection();

        app.UseRouting();

        app.UseAuthentication();

        app.UseMiddleware<IopTokenLifetimeValidationMiddleware>();

        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
            endpoints.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
            });
        });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddIopCoreServices(Configuration, Environment.EnvironmentName, ClientGeneratorEnvironmentName);

        services.AddControllers(options =>
        {
            var isReadOnlyStringValue = Configuration.GetValue<string>("ReadOnly");

            _ = bool.TryParse(isReadOnlyStringValue, out bool isReadOnly);

            if (isReadOnly)
            {
                options.Filters.Add<ReadOnlyModeFilter>();
            }
        })
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
            options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        services.AddProblemDetails(ConfigureProblemDetails);

        services.AddSwaggerGen(AddSwagger);

        if (!Environment.EnvironmentName.Equals(ClientGeneratorEnvironmentName))
        {
            services.TryAddSecurity(Configuration, Environment);

            services.AddLuceneSearch();
        }

        services.AddHealthChecks()
            .AddCheck<DatabaseHealthCheck>("Database");

        services.AddSingleton<IAuthorizationMiddlewareResultHandler, IopAuthorizationMiddlewareResultHandler>();
    }

    private static ProblemDetails MapValidationException(ValidationException exception)
    {
        if (exception.Errors.All(x => x.ErrorCode == "Forbidden"))
        {
            return new ProblemDetails { Type = "https://httpstatuses.com/403", Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = exception.Message };
        }
        if (exception.Errors.All(x => x.ErrorCode == "NoAuthorization"))
        {
            return new ProblemDetails { Type = "https://httpstatuses.com/401", Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized", Detail = exception.Message };
        }
        if (exception.Errors.All(x => x.ErrorCode == "NotFound" || x.ErrorCode == "NoAuthorization"))
        {
            return new ProblemDetails { Type = "https://httpstatuses.com/404", Status = StatusCodes.Status404NotFound, Title = "Not Found", Detail = exception.Message };
        }

        return new ProblemDetails { Type = "https://httpstatuses.com/400", Status = StatusCodes.Status400BadRequest, Title = "Bad Request", Detail = exception.Message };
    }

    private void AddSwagger(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions c)
    {
        c.SchemaFilter<EnumSchemaFilter>();

        c.SwaggerDoc(_swaggerInformation.Version, new OpenApiInfo
        {
            Title = _swaggerInformation.Title,
            Version = _swaggerInformation.Version,
            Description = _swaggerInformation.Description
        });

        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = @"JWT Authorization header. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
            Name = "Authorization",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.ApiKey,
            Scheme = "Bearer"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

        // Set the comments path for the Swagger JSON and UI.
        var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
        var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        c.IncludeXmlComments(xmlPath, includeControllerXmlComments: false);
    }

    private void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (_, _) => Environment.IsDevelopment();

        options.Map<BadRequestException>(x => new ProblemDetails { Type = "https://httpstatuses.com/400", Status = StatusCodes.Status400BadRequest, Title = "Bad Request", Detail = x.Message });
        options.Map<ArgumentException>(x => new ProblemDetails { Type = "https://httpstatuses.com/400", Status = StatusCodes.Status400BadRequest, Title = "Bad Request", Detail = x.Message });
        options.Map<ValidationException>(x => MapValidationException(x));
        options.Map<NotFoundException>(x => new ProblemDetails { Type = "https://httpstatuses.com/404", Status = StatusCodes.Status404NotFound, Title = "Not Found", Detail = x.Message });
        options.Map<ForbiddenException>(x => new ProblemDetails { Type = "https://httpstatuses.com/403", Status = StatusCodes.Status403Forbidden, Title = "Forbidden", Detail = x.Message });
        options.Map<UnauthorizedException>(x => new ProblemDetails { Type = "https://httpstatuses.com/401", Status = StatusCodes.Status401Unauthorized, Title = "Unauthorized", Detail = x.Message });
        options.Map<MethodNotAllowedException>(x => new ProblemDetails { Type = "https://httpstatuses.com/405", Status = StatusCodes.Status405MethodNotAllowed, Title = "Method Not Allowed", Detail = x.Message });
        options.Map<ConflictException>(x => new ProblemDetails { Type = "https://httpstatuses.com/409", Status = StatusCodes.Status409Conflict, Title = "Conflict", Detail = x.Message });
        // Entity Framework wraps failures it considers transient in an InvalidOperationException,
        // so the mapping cannot key on the outermost exception type; CanMap walks the whole chain.
        options.Map<Exception>(
            (_, exception) => DatabaseErrorMapper.CanMap(exception),
            (_, exception) => DatabaseErrorMapper.ToProblemDetails(exception));
        options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
    }
}