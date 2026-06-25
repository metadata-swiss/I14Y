using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AccessUrlMappingProfiles : Profile
{
    public AccessUrlMappingProfiles()
        : base(nameof(AccessUrlMappingProfiles))
    {
        CreateMap<Models.AccessUrl, ResourceModel>()
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label))
            .ForMember(d => d.Uri, opt => opt.MapFrom(s => s.Href));

        CreateMap<ResourceModel, Models.AccessUrl>()
            .ForMember(d => d.IsDownload, opt => opt.Ignore())
            .ForMember(d => d.Href, opt => opt.MapFrom(s => s.Uri))
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Label));
    }
}