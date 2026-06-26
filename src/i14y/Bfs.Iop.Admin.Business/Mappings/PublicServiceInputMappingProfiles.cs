using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PublicServiceInputMappingProfiles : Profile
{
    public PublicServiceInputMappingProfiles()
        : base(nameof(PublicServiceInputMappingProfiles))
    {
        CreateMap<PublicServiceModel, PublicServiceInput>()
            .ForMember(d => d.BusinessEventsCodes, opt => opt.MapFrom(s => s.BusinessEvents.Select(x => x.Code)))
            .ForMember(d => d.Channels, opt => opt.MapFrom(s => s.Channels))
            .ForMember(d => d.CompetentAuthority, opt => opt.MapFrom(s => s.Publisher))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.LanguageCodes, opt => opt.MapFrom(s => s.Languages.Select(x => x.Code)))
            .ForMember(d => d.LifeEventsCodes, opt => opt.MapFrom(s => s.LifeEvents.Select(x => x.Code)))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.SectorCodes, opt => opt.MapFrom(s => s.Sectors.Select(x => x.Code)))
            .ForMember(d => d.Spatial, opt => opt.MapFrom(s => s.Spatial))
            .ForMember(d => d.ThematicAreaCodes, opt => opt.MapFrom(s => s.ThematicAreas.Select(x => x.Code)))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.SpatialCH, opt => opt.MapFrom(s => s.SpatialCH));

        CreateMap<PublicServiceInput, PublicServiceInputModel>()
            .ForMember(d => d.BusinessEvents, opt => opt.MapFrom(s => s.BusinessEventsCodes.Select(x => new CodeInputModel() { Code = x })))
            .ForMember(d => d.Channels, opt => opt.MapFrom(s => s.Channels))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.IsDescribedAt, opt => opt.MapFrom(s => s.IsDescribedAt))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.LanguageCodes.Select(x => new CodeInputModel() { Code = x })))
            .ForMember(d => d.LifeEvents, opt => opt.MapFrom(s => s.LifeEventsCodes.Select(x => new CodeInputModel() { Code = x })))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Publisher, opt => opt.MapFrom(s => s.CompetentAuthority))
            .ForMember(d => d.Relations, opt => opt.MapFrom(s => s.Relations))
            .ForMember(d => d.Requires, opt => opt.MapFrom(s => s.Requires))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.Sectors, opt => opt.MapFrom(s => s.SectorCodes.Select(x => new CodeInputModel() { Code = x })))
            .ForMember(d => d.Spatial, opt => opt.MapFrom(s => s.Spatial))
            .ForMember(d => d.SpatialCH, opt => opt.MapFrom(s => s.SpatialCH.Select(x => new CodeInputModel() { Code = x.Code })))
            .ForMember(d => d.ThematicAreas, opt => opt.MapFrom(s => s.ThematicAreaCodes.Select(x => new CodeInputModel() { Code = x })));
    }
}