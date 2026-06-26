using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Bfs.Iop.Admin.GeocatClient.UnitTests;

public class GeocatClientMappingProfileTest
{
    [Test]
    public void Automapper_Configuration_IsValid()
    {
        var _config = new MapperConfiguration(configure => configure.AddProfile<GeocatClientMappingProfile>(), NullLoggerFactory.Instance);
        _config.AssertConfigurationIsValid();
    }
}