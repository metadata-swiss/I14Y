using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PublicServiceViewMappingProfiles : Profile
{
    public PublicServiceViewMappingProfiles() : base(nameof(PublicServiceViewMappingProfiles))
    {
        CreateMap<PublicServiceModel, PublicServiceView>()
            .ForMember(d => d.BusinessEvents, opt => opt.MapFrom(s => s.BusinessEvents))
            .ForMember(d => d.Channels, opt => opt.MapFrom(s => s.Channels))
            .ForMember(d => d.CompetentAuthority, opt => opt.MapFrom(s => s.Publisher))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.IsDescribedAt, opt => opt.Ignore())
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.Languages))
            .ForMember(d => d.LifeEvents, opt => opt.MapFrom(s => s.LifeEvents))
            .ForMember(d => d.PublicationLevel, opt => opt.MapFrom(s => s.PublicationLevel))
            .ForMember(d => d.PublicationLevelProposal, opt => opt.MapFrom(s => s.PublicationLevelProposal))
            .ForMember(d => d.RegistrationStatus, opt => opt.MapFrom(s => s.RegistrationStatus))
            .ForMember(d => d.RegistrationStatusProposal, opt => opt.MapFrom(s => s.RegistrationStatusProposal))
            .ForMember(d => d.Relation, opt => opt.Ignore())
            .ForMember(d => d.Requires, opt => opt.Ignore())
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.Sectors, opt => opt.MapFrom(s => s.Sectors))
            .ForMember(d => d.Spatial, opt => opt.MapFrom(s => s.Spatial))
            .ForMember(d => d.SpatialCH, opt => opt.MapFrom(s => s.SpatialCH))
            .ForMember(d => d.System, opt => opt.MapFrom(s => s.System))
            .ForMember(d => d.ThematicAreas, opt => opt.MapFrom(s => s.ThematicAreas))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name));

        CreateMap<PublicServiceModel, IdLabel>()
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Label, opt => opt.MapFrom(s => s.Name));
    }
}