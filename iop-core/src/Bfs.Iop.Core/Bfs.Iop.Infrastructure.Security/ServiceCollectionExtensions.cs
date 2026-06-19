using Bfs.Iop.Infrastructure.Security.Configuration;
using Bfs.Iop.Infrastructure.Security.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Net.Http.Headers;
using System.IdentityModel.Tokens.Jwt;

namespace Bfs.Iop.Infrastructure.Security;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(configuration, nameof(configuration));

        if (services.Any(x => x.ServiceType == typeof(IUserContextService)))
        {
            return services;
        }

        services.AddHttpContextAccessor();
        services.AddAuthorization();

        services.RegisterSecurityConfiguration(configuration);        

        services.AddScoped<IAuthorizationProvider, AuthorizationProvider>();
        services.AddScoped<IUserContextService, UserContextService>();

        return services;
    }

    private static IServiceCollection RegisterSecurityConfiguration(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        const string EiamKey = "Eiam";
        const string KeycloakKey = "Keycloak";

        var eiamConfig = new EiamConfiguration();
        configuration.Bind(EiamKey, eiamConfig);

        var keycloakConfig = new KeycloakConfiguration();
        configuration.Bind(KeycloakKey, keycloakConfig);

        var securityConfiguration = new SecurityConfiguration();
        securityConfiguration.AddConfiguration(eiamConfig);
        securityConfiguration.AddConfiguration(keycloakConfig);

        var authBuilder = services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(RegisterMainJwt);

        foreach (var config in securityConfiguration.Configurations)
        {
            foreach (var issuer in config.Issuers)
            {
                authBuilder.AddJwtBearer(issuer, options => RegisterIssuerJwt(issuer, config.Audience, options));
            }
        }

        services.AddSingleton<ISecurityConfiguration>(securityConfiguration);

        return services;
    }

    private static void RegisterMainJwt(JwtBearerOptions options)
    {
        options.RequireHttpsMetadata = false;

        options.ForwardDefaultSelector = context =>
        {
            var authorizationValues = context.Request.Headers[HeaderNames.Authorization];
            var authorization = authorizationValues.FirstOrDefault();

            var securityConfiguration = context.RequestServices.GetRequiredService<ISecurityConfiguration>();

            if (!string.IsNullOrEmpty(authorization) && 
                authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                var token = authorization.Substring("Bearer ".Length).Trim();

                var jwtHandler = new JwtSecurityTokenHandler();

                if (jwtHandler.CanReadToken(token))
                {
                    var jwtToken = jwtHandler.ReadJwtToken(token);

                    if (securityConfiguration.Configurations.SelectMany(x => x.Issuers).Contains(jwtToken.Issuer))
                    {
                        return jwtToken.Issuer;
                    }
                }
            }

            return null;
        };
    }

    private static void RegisterIssuerJwt(string issuer, string audience, JwtBearerOptions options)
    {
        options.Authority = issuer;
        options.Audience = audience;
        options.TokenValidationParameters.ValidAudiences = [audience];
        options.TokenValidationParameters.ValidateAudience = true;
        options.TokenValidationParameters.ValidateIssuer = true;
        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
        options.TokenValidationParameters.ValidIssuer = issuer;
        options.MapInboundClaims = false;
    }
}
