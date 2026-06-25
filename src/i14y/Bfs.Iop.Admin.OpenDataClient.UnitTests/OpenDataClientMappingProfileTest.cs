using AutoMapper;
using Microsoft.Extensions.Logging.Abstractions;
using NUnit.Framework;

namespace Bfs.Iop.Admin.OpenDataClient.UnitTests;

public class OpenDataClientMappingProfileTest
{
    [Test]
    public void Automapper_Configuration_IsValid()
    {
        var _config = new MapperConfiguration(configure => configure.AddProfile<OpenDataClientMappingProfile>(), NullLoggerFactory.Instance);
        _config.AssertConfigurationIsValid();
    }
}