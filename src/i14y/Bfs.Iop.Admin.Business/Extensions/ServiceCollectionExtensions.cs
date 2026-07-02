using Bfs.Iop.Admin.Business.ExternalClients.EIAM;
using Bfs.Iop.Admin.Business.Mappings;
using Bfs.Iop.Admin.Business.Services;
using Bfs.Iop.Admin.Business.Validation;
using Bfs.Iop.Admin.Models;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace Bfs.Iop.Admin.Business.Extensions;

/// <summary>
/// Register business related services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds IOP Admin business services.
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    public static IServiceCollection AddBfsIopAdminBusiness(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ServiceCollectionExtensions).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(ServiceCollectionExtensions).Assembly);

        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        services.AddSingleton(x => configuration.GetSection(LocalizerConfiguration.SectionName).Get<LocalizerConfiguration>() 
            ?? throw new Exception("Localizer configuration not found!"));

        services.AddSingleton<ILocalizerService, LocalizerService>();

        return services;
    }

    /// <summary>
    /// Setup the HTTP client factory and known clients.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <returns>The service collection.</returns>
    public static IServiceCollection SetupHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        const string EIAMConfigUri = "EIAMConfig:Uri";

        services.AddHttpClient(EIAMSoapApiClient.ClientName, config =>
        {
            config.BaseAddress = new Uri(configuration[EIAMConfigUri] ?? throw new Exception($"{nameof(EIAMConfigUri)} configuration value is null."));
        });

        services.AddTransient<IEIAMSoapApiClient, EIAMSoapApiClient>();

        return services;
    }
}