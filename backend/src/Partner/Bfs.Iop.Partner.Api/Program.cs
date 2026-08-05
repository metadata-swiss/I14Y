using Azure.Identity;
using Bfs.Iop.Core.ApiClient;
using Bfs.Iop.Core.ApiClient.Extensions;
using Bfs.Iop.Core.ApiClient.Health;
using Bfs.Iop.Core.Common.Api.Extensions;
using Bfs.Iop.Infrastructure.ApiClient;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Partner.Api.Authentication;
using Bfs.Iop.Partner.Api.Middleware;
using Bfs.Iop.Partner.Api.Swagger;
using Bfs.Iop.Partner.Business.Extensions;
using Bfs.Iop.Partner.Json;
using HealthChecks.UI.Client;
using Hellang.Middleware.ProblemDetails;
using Lamar.Microsoft.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using System.Reflection;
using System.Text.Json.Serialization;
using ProblemDetailsOptions = Hellang.Middleware.ProblemDetails.ProblemDetailsOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseLamar(); // Needed to resolve types from Bfs.Sis.Common.Api

builder.WebHost
    .ConfigureKestrel(options => options.AddServerHeader = false)
    .ConfigureAppConfiguration((context, config) =>
    {
        var env = context.HostingEnvironment;

        if (!env.IsDevelopment()) //this is only relevant for Azure, possible environments DEV, ABN, PRD
        {
            var environment = env.EnvironmentName.ToLowerInvariant();
            // attach Azure services, build what we have so far (appsettings.*, env vars, secrets.json, etc.)
            IConfiguration built = config.Build();

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
            built = config.Build();
        }
    });

builder.Logging
    .ClearProviders()
    .AddConfiguration(builder.Configuration.GetSection("Logging"))
    .AddConsole()
    .SetMinimumLevel(LogLevel.Trace);

builder.Services.AddTransient<ITokenRetriever, IopCoreAccessTokenProvider>();
builder.Services.AddIopCoreApiClient(builder.Configuration.GetValue<string>("DcatUrl")
    ?? throw new NullReferenceException("DcatUrl"));

builder.Services.AddControllers()
    .AddJsonOptions(options => 
    {
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;   
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.Converters.Add(new JsonConceptInputConverter());
    });

var isDevelopment = builder.Environment.IsDevelopment();
builder.Services.AddProblemDetails(x => ConfigureProblemDetails(x, isDevelopment));

var _swaggerInformation = new SwaggerInformation(
    builder.Environment.EnvironmentName,
    builder.Configuration.GetValue<string>("APP_VERSION") ?? string.Empty);

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(AddSwagger); //analog dcat
builder.Services.AddSwaggerExamplesFromAssemblies(Assembly.GetEntryAssembly());

builder.Services.AddHealthChecks()
    .AddCheck<IopCoreApiClientHealthCheck>("Iop Core");

builder.Services.TryAddSecurity(builder.Configuration, builder.Environment);

builder.Services.AddRouting(options => options.LowercaseUrls = true);

builder.Services.AddCors();

var app = builder.Build();

// Use HTTP traffic logger, which allows to log all requests and responses.
app.UseMiddleware<HttpTrafficLogger>();

// Apply Exception Mappings.
app.UseProblemDetails();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.DefaultModelsExpandDepth(-1);
    c.SwaggerEndpoint($"/swagger/{_swaggerInformation.Version}/swagger.json", $"{_swaggerInformation.Title} {_swaggerInformation.Version}");
    c.RoutePrefix = "api";
    c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
});

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader()
    .WithExposedHeaders(
        HttpContextExtensions.PageHeaderKey,
        HttpContextExtensions.PageSizeHeaderKey,
        HttpContextExtensions.TotalPagesHeaderKey,
        HttpContextExtensions.TotalRowsHeaderKey,
        "Content-Disposition"
));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions()
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();

void ConfigureProblemDetails(ProblemDetailsOptions options, bool isDevelopmentEnvironment)
{
    options.IncludeExceptionDetails = (_, _) => isDevelopmentEnvironment;

    options.Map<ArgumentException>(x => new ProblemDetails { Type = "https://httpstatuses.com/400", Status = StatusCodes.Status400BadRequest, Title = "Bad Request", Detail = x.Message });

    options.Map<ApiException>(x =>
    {
        if (x is ApiException<ProblemDetails> apiException)
        {
            apiException.Result.Title = "Forwarded: " + apiException.Result.Title;
            return apiException.Result;
        }

        return new ProblemDetails { Type = $"https://httpstatuses/{x.StatusCode}", Status = x.StatusCode, Title = "Forwarded", Detail = x.Message };
    });

    options.MapToStatusCode<Exception>(StatusCodes.Status500InternalServerError);
}

void AddSwagger(Swashbuckle.AspNetCore.SwaggerGen.SwaggerGenOptions options)
{
    options.SwaggerDoc(_swaggerInformation.Version, new OpenApiInfo
    {
        Title = _swaggerInformation.Title,
        Version = _swaggerInformation.Version,
        Description = _swaggerInformation.Description
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

    options.UseAllOfForInheritance();
    options.UseOneOfForPolymorphism();

    options.ExampleFilters();

    // Set the comments path for the Swagger JSON and UI.
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    options.IncludeXmlComments(xmlPath, includeControllerXmlComments: false);

    options.CustomSchemaIds(type => type.ToString());
}