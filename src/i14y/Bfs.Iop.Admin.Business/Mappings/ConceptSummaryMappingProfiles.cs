using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal sealed class ConceptSummaryMappingProfiles : Profile
{
    public ConceptSummaryMappingProfiles()
        : base(nameof(ConceptSummaryMappingProfiles))
    {
        CreateMap<VocabularyEntryModel, Models.FilterCountResultItem>()
            .ForMember(t => t.Reference, opt => opt.MapFrom(s => s.Code))
            .ForMember(t => t.Count, x => x.Ignore())
            .ForMember(t => t.Label, opt => opt.MapFrom(s => s.Name))
            ;
    }
}