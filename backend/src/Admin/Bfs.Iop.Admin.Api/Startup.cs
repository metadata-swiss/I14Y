using Bfs.Iop.Admin.Api.Attributes;
using Bfs.Iop.Admin.Api.Authentication;
using Bfs.Iop.Admin.Api.Config;
using Bfs.Iop.Admin.Api.Extensions;
using Bfs.Iop.Admin.Api.MediatR;
using Bfs.Iop.Admin.Api.Swagger;
using Bfs.Iop.Admin.Business.Extensions;
using Bfs.Iop.Admin.GeocatClient;
using Bfs.Iop.Admin.OpenDataClient;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.ApiClient.Extensions;
using Bfs.Iop.Core.ApiClient.Health;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Infrastructure.Security;
using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using MediatR.Pipeline;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Events;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

namespace Bfs.Iop.Admin.Api;

/// <summary>
/// Start up the application.
/// </summary>
public class Startup
{
    /// <summary>
    /// The client generator environment name.
    /// </summary>
    public const string ClientGeneratorEnvironmentName = "WebApiClientGenerator";

    /// <summary>
    /// The in-memory test environment name.
    /// </summary>
    public const string InMemoryTestEnvironmentName = "InMemoryTest";

    /// <summary>
    /// Initializes a <see cref="Startup"/> instance.
    /// </summary>
    /// <param name="configuration"></param>
    /// <param name="env"></param>
    public Startup(IConfiguration configuration, IWebHostEnvironment env)
    {
        Configuration = configuration;
        Environment = env;
    }

    /// <inheritdoc cref="IConfiguration" />
    public IConfiguration Configuration { get; }

    /// <inheritdoc cref="IWebHostEnvironment" />
    public IWebHostEnvironment Environment { get; }

    /// <summary>
    /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    /// </summary>
    /// <param name="app"></param>
    public void Configure(IApplicationBuilder app)
    {
        app.UseStaticFiles();

        app.UseSerilogRequestLogging(options =>
        {
            options.IncludeQueryInRequestPath = true; // adds ?query=... to {RequestPath}

            options.GetLevel = (httpContext, elapsedMs, ex) =>
            {
                if (ex != null)
                    return LogEventLevel.Error;

                var statusCode = httpContext.Response.StatusCode;

                if (statusCode >= 500)
                    return LogEventLevel.Error;      // server errors
                if (statusCode >= 400)
                    return LogEventLevel.Warning;    // client errors

                return LogEventLevel.Debug;          // hide successful requests
            };

            options.MessageTemplate =
                "HTTP {RequestMethod} {RequestPath} from {ClientIP} ({Host}) UA={UserAgent} " +
                "responded {StatusCode} in {Elapsed:0.0000} ms";

            options.EnrichDiagnosticContext = (diag, ctx) =>
            {
                var req = ctx.Request;
                var conn = ctx.Connection;

                diag.Set("Host", req.Host.Value);
                diag.Set("ClientIP", conn.RemoteIpAddress?.ToString());
                diag.Set("UserAgent", req.Headers.UserAgent.ToString());
            };
        });

        app.MapWhen(c => true, backendApp =>
        {
            backendApp.UseSecurityHeaders();
            backendApp.UseProblemDetails();
            backendApp.UseSwagger();

            SwaggerInformation
                .GetSwaggerEnvironments(Configuration, Environment)
                .Where(environment => environment.SwaggerEndpoints.Any())
                .ToList()
                .ForEach(environment => backendApp.UseSwaggerUI(option =>
                {
                    option.DefaultModelsExpandDepth(-1);

                    foreach (var swaggerEndpoint in environment.SwaggerEndpoints)
                    {
                        option.SwaggerEndpoint(swaggerEndpoint.Url, swaggerEndpoint.Name);
                    }

                    option.RoutePrefix = environment.RoutePrefix;
                    option.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
                }));

            backendApp.UseHttpsRedirection();

            backendApp.UseRouting();

            backendApp.UseCors(CorsConfig.PolicyName);

            backendApp.UseAuthentication();

            backendApp.UseAuthorization();

            backendApp.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/.well-known/security.txt", context =>
                {
                    context.Response.Redirect("https://admin.ch/.well-known/security.txt");
                    return Task.CompletedTask;
                });

                endpoints.MapControllers();
                endpoints.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
                {
                    Predicate = (HealthCheckRegistration _) => true,
                    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
                });
            });
        });
    }

    /// <summary>
    /// This method gets called by the runtime. Use this method to add services to the container.
    /// </summary>
    /// <param name="services"></param>
    public void ConfigureServices(IServiceCollection services)
    {
        services.ConfigureCors(Configuration);

        services.AddControllers()
            .AddJsonOptions(options => options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        services.AddBfsIopAdminBusiness(Configuration);

        services.AddTransient<ITokenRetriever, RequestUserTokenRetriever>();
        services.AddIopCoreApiClient(Configuration.GetValue<string>("DcatUrl"));

        services.AddProblemDetails(ConfigureProblemDetails);

        services.AddTransient(typeof(IRequestExceptionHandler<,,>), typeof(CommandHandlersExceptionHandler<,,>));

        services.AddSwaggerGen(options =>
        {
            SwaggerInformation.GetSwaggerEnvironments(Configuration, Environment)
                .Where(environment => environment.GenerateSwaggerDoc)
                .ToList()
                .ForEach(environment => options.SwaggerDoc(
                    environment.Version,
                    new OpenApiInfo
                    {
                        Title = environment.Title,
                        Version = environment.Version,
                        Description = environment.Description
                    }));

            options.DocInclusionPredicate((docName, apiDesc) =>
            {
                if (!apiDesc.TryGetMethodInfo(out MethodInfo methodInfo))
                {
                    return false;
                }

                var versionAttributes = methodInfo.DeclaringType?
                    .GetCustomAttributes(true)
                       .OfType<SwaggerVersionAttribute>();

                return versionAttributes == null || (!versionAttributes.Any() && docName == SwaggerInformation.V1) || versionAttributes.Any(attr => attr.Version == docName);
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header. Enter 'Bearer' [space] and then your token in the text input below. Example: 'Bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer"
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
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
            options.OperationFilter<FileResultContentTypeOperationFilter>();
            options.OperationFilter<CatchAllOperationFilter>();
            options.OperationFilter<ReApplyOptionalRouteParameterOperationFilter>();
            options.UseAllOfToExtendReferenceSchemas();

            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
            options.IncludeXmlComments(xmlPath, includeControllerXmlComments: false);
        });

        services.AddTransient<HttpClient>();

        services.SetupHttpClients(Configuration);

        if (!Environment.EnvironmentName.Equals(ClientGeneratorEnvironmentName))
        {
            services.TryAddSecurity(Configuration);
        }

        services.AddGeocatClient(Configuration.GetSection("GeocatClient"));
        services.AddOpenDataClient(Configuration.GetSection("OpenDataClient"));

        services.AddHealthChecks()
            .AddCheck<IopCoreApiClientHealthCheck>("Iop Core");
    }

    private void ConfigureProblemDetails(ProblemDetailsOptions options)
    {
        options.IncludeExceptionDetails = (ctx, ex) =>
        {
            return Environment.IsDevelopment();
        };

        options.Map<ArgumentException>(x => x.GetProblemDetails(HttpStatusCode.BadRequest));

        options.Map<BadRequestException>(x => x.GetProblemDetails(HttpStatusCode.BadRequest));
        options.Map<ForbiddenException>(x => x.GetProblemDetails(HttpStatusCode.Forbidden));
        options.Map<NotFoundException>(x => x.GetProblemDetails(HttpStatusCode.NotFound));
        options.Map<UnauthorizedException>(x => x.GetProblemDetails(HttpStatusCode.Unauthorized));

        options.Map<FluentValidation.ValidationException>(x => x.GetProblemDetails());
        options.Map<ApiException>(x =>
        {
            if (x is ApiException<ProblemDetails> apiException)
            {
                apiException.Result.Title = "Forwarded: " + apiException.Result.Title;
                return apiException.Result;
            }

            return new ProblemDetails
            {
                Type = $"https://httpstatuses/{x.StatusCode}",
                Status = x.StatusCode,
                Title = "Forwarded",
                Detail = x.Message
            };
        });

        options.Map<InvalidOperationException>(x =>
        {
            return new ProblemDetails
            {
                Type = $"https://httpstatuses/500",
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = x.Message
            };
        });

        options.MapToStatusCode<Exception>((int)HttpStatusCode.InternalServerError);
    }
}