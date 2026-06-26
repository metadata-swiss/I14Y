using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class MultiLanguageMappingProfiles : Profile
{
    public MultiLanguageMappingProfiles()
        : base(nameof(MultiLanguageMappingProfiles))
    {
        CreateMap<MultiLanguageModel, MultiLanguage>();

        CreateMap<MultiLanguage, MultiLanguageModel>();

        CreateMap<string, MultiLanguage>()
            .ForMember(d => d.En, opt => opt.MapFrom(s => s))
            .ForMember(d => d.De, opt => opt.MapFrom(s => s))
            .ForMember(d => d.Fr, opt => opt.MapFrom(s => s))
            .ForMember(d => d.It, opt => opt.MapFrom(s => s))
            .ForMember(d => d.Rm, opt => opt.MapFrom(s => s))
            ;

        CreateMap<MultiLanguage, MultiLanguageModel>();
    }
}