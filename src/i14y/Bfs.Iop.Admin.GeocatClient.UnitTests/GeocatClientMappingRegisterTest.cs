using Mapster;
using NUnit.Framework;

namespace Bfs.Iop.Admin.GeocatClient.UnitTests;

public class GeocatClientMappingRegisterTest
{
    [Test]
    public void Mapster_Configuration_IsValid()
    {
        Assert.DoesNotThrow(() =>
        {
            TypeAdapterConfig.GlobalSettings.Scan(typeof(GeocatClientMappingRegister).Assembly);
            TypeAdapterConfig.GlobalSettings.Compile();
        });
    }
}