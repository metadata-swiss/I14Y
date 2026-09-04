using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PublicServiceInputMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PublicServiceModel, PublicServiceInput>()
            .Map(dest => dest.BusinessEventsCodes, src => src.BusinessEvents.Select(x => x.Code))
            .Map(dest => dest.Channels, src => src.Channels)
            .Map(dest => dest.CompetentAuthority, src => src.Publisher)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.LanguageCodes, src => src.Languages.Select(x => x.Code))
            .Map(dest => dest.LifeEventsCodes, src => src.LifeEvents.Select(x => x.Code))
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.SectorCodes, src => src.Sectors.Select(x => x.Code))
            .Map(dest => dest.Spatial, src => src.Spatial)
            .Map(dest => dest.ThematicAreaCodes, src => src.ThematicAreas.Select(x => x.Code))
            .Map(dest => dest.Title, src => src.Name)
            .Map(dest => dest.SpatialCH, src => src.SpatialCH);

        config.NewConfig<PublicServiceInput, PublicServiceInputModel>()
            .Map(dest => dest.BusinessEvents, src => src.BusinessEventsCodes.Select(x => new CodeInputModel { Code = x }))
            .Map(dest => dest.Channels, src => src.Channels)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.IsDescribedAt, src => src.IsDescribedAt)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.Languages, src => src.LanguageCodes.Select(x => new CodeInputModel { Code = x }))
            .Map(dest => dest.LifeEvents, src => src.LifeEventsCodes.Select(x => new CodeInputModel { Code = x }))
            .Map(dest => dest.Name, src => src.Title)
            .Map(dest => dest.Publisher, src => src.CompetentAuthority)
            .Map(dest => dest.Relations, src => src.Relations)
            .Map(dest => dest.Requires, src => src.Requires)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.Sectors, src => src.SectorCodes.Select(x => new CodeInputModel { Code = x }))
            .Map(dest => dest.Spatial, src => src.Spatial)
            .Map(dest => dest.SpatialCH, src => src.SpatialCH.Select(x => new CodeInputModel { Code = x.Code }))
            .Map(dest => dest.ThematicAreas, src => src.ThematicAreaCodes.Select(x => new CodeInputModel { Code = x }));
    }
}