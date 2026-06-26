using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AgentMappingProfiles : Profile
{
    public AgentMappingProfiles()
        : base(nameof(AgentMappingProfiles))
    {
        CreateMap<Models.Agent, IdentifierInputModel>()
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier));

        CreateMap<AgentModel, Models.Agent>()
            .ForMember(d => d.Classification, opt => opt.MapFrom(s => s.Classification))
            .ForMember(d => d.ContactPoint, opt => opt.MapFrom(s => s.ContactPoint))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.HomePage, opt => opt.MapFrom(s => s.HomePage))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Images, opt => opt.MapFrom(s => s.Images))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.PrefLabel, opt => opt.MapFrom(s => s.PrefLabel))
            .ForMember(d => d.Spatial, opt => opt.MapFrom(s => s.Spatial))
            .ForMember(d => d.SpatialCH, opt => opt.MapFrom(s => s.SpatialCH))
            .ForMember(d => d.SubAgents, opt => opt.MapFrom(s => s.SubAgents))
            .ForMember(d => d.SubAgentOf, opt => opt.Ignore())
            .ForMember(d => d.System, opt => opt.MapFrom(s => s.System))
            .ForMember(d => d.Uid, opt => opt.MapFrom(s => s.Uid));

        CreateMap<AgentModel, IdNameModel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name));

        CreateMap<AgentModel, IdentifierInputModel>()
            .ForMember(dest => dest.Identifier, opt => opt.MapFrom(src => src.Identifier))
            ;
    }
}