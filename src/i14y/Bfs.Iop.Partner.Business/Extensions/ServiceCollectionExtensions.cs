using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Partner.Business.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection SetupIopPartnerBusinessLogic(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        var assembly = typeof(ServiceCollectionExtensions).Assembly;

        return services
            .AddAutoMapper(_ => { }, assembly);
    }
}