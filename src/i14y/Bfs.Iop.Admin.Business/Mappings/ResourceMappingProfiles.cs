using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ResourceMappingProfiles : Profile
{
    public ResourceMappingProfiles()
        : base(nameof(ResourceMappingProfiles))
    {
        CreateMap<Resource, ResourceModel>()
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label))
            .ForMember(d => d.Uri, opt => opt.MapFrom(s => s.Href));

        CreateMap<ResourceModel, Resource>()
            .ForMember(d => d.Href, opt => opt.MapFrom(s => s.Uri))
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));
    }
}