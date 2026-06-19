using Bfs.Iop.Core.Data.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;

namespace Bfs.Iop.Core.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection TryAddIopDbContext(
        this IServiceCollection services,
        Action<DbContextOptionsBuilder> dbContextOptionsBuilderDelegate,
        ServiceLifetime serviceLifetime = ServiceLifetime.Scoped)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));
        ArgumentNullException.ThrowIfNull(dbContextOptionsBuilderDelegate, nameof(dbContextOptionsBuilderDelegate));

        if (services.FirstOrDefault(x => x.ServiceType == typeof(IopDbContext)) is null)
        {
            services.AddDbContext<IopDbContext>(dbContextOptionsBuilderDelegate, serviceLifetime, serviceLifetime);
            services.Add(new ServiceDescriptor(typeof(IIopDatabaseMigrator), typeof(IopDatabaseMigrator), serviceLifetime));
            services.AddTransient<DbContext>(sp => sp.GetRequiredService<IopDbContext>());
        }

        return services;
    }
}
