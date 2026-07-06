using Bfs.Iop.Admin.Business.Mappings;
using Mapster;
using MapsterMapper;
using Microsoft.Extensions.DependencyInjection;

namespace Bfs.Iop.Admin.Business.UnitTests;

public static class TestHelper
{
    public static IMapper CreateMapper()
    {
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(typeof(MultiLanguageMappingRegister).Assembly);

        var services = new ServiceCollection();
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        var serviceProvider = services.BuildServiceProvider();

        return serviceProvider.GetRequiredService<IMapper>();
    }
}