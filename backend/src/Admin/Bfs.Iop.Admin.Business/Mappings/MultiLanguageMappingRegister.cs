using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class MultiLanguageMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MultiLanguageModel, MultiLanguage>();

        config.NewConfig<MultiLanguage, MultiLanguageModel>();

        config.NewConfig<string, MultiLanguage>()
            .Map(dest => dest.En, src => src)
            .Map(dest => dest.De, src => src)
            .Map(dest => dest.Fr, src => src)
            .Map(dest => dest.It, src => src)
            .Map(dest => dest.Rm, src => src);
    }
}