using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class IdLabelMappingProfiles : Profile
{
    public IdLabelMappingProfiles()
        : base(nameof(IdLabelMappingProfiles))
    {
        CreateMap<DataServiceModel, Models.IdLabel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(t => t.Label, opt => opt.MapFrom(s => s.Title))
            ;
    }
}