using AutoMapper;
using Bfs.Iop.Partner.Business.Mappings;
using Microsoft.Extensions.Logging.Abstractions;

namespace Bfs.Iop.Partner.Business.UnitTests.Helpers;

public static class TestsHelper
{
    public static MapperConfiguration GetFullMapperConfiguration() => 
    new MapperConfiguration(cfg => cfg.AddMaps(typeof(ConceptInputProfiles)), NullLoggerFactory.Instance);
}
