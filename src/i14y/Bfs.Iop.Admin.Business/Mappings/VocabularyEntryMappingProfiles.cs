using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class VocabularyEntryMappingProfiles : Profile
{
    public VocabularyEntryMappingProfiles()
        : base(nameof(VocabularyEntryMappingProfiles))
    {
        CreateMap<VocabularyEntryModel, Models.VocabularyEntry>();

        CreateMap<Models.VocabularyEntry, VocabularyEntryModel>();

        CreateMap<Models.VocabularyEntry, CodeInputModel>()
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.Code));

        CreateMap<CodeInputModel, Models.VocabularyEntry>()
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.Code));
    }
}