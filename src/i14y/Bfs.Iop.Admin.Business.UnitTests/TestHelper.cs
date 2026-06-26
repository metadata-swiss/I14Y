using AutoMapper;
using Bfs.Iop.Admin.Business.Mappings;

namespace Bfs.Iop.Admin.Business.UnitTests;

public static class TestHelper
{
    public static MapperConfiguration GetFullMapperConfiguration() => 
        new(cfg => cfg.AddMaps(typeof(MultiLanguageMappingProfiles)), Microsoft.Extensions.Logging.Abstractions.NullLoggerFactory.Instance);
}