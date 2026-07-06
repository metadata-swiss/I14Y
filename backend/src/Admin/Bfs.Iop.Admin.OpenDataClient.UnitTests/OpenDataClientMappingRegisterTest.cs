using Mapster;
using NUnit.Framework;

namespace Bfs.Iop.Admin.OpenDataClient.UnitTests;

public class OpenDataClientMappingRegisterTest
{
    [Test]
    public void Automapper_Configuration_IsValid()
    {
        Assert.DoesNotThrow(() =>
        {
            TypeAdapterConfig.GlobalSettings.Scan(typeof(OpenDataClientMappingRegister).Assembly);
            TypeAdapterConfig.GlobalSettings.Compile();
        });
    }
}