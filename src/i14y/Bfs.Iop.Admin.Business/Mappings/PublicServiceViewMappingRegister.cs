using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PublicServiceViewMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<PublicServiceModel, PublicServiceView>()
            .Map(d => d.BusinessEvents, s => s.BusinessEvents)
            .Map(d => d.Channels, s => s.Channels)
            .Map(d => d.CompetentAuthority, s => s.Publisher)
            .Map(d => d.Description, s => s.Description)
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Identifiers, s => s.Identifiers)
            .Ignore(d => d.IsDescribedAt)
            .Map(d => d.Keywords, s => s.Keywords)
            .Map(d => d.Languages, s => s.Languages)
            .Map(d => d.LifeEvents, s => s.LifeEvents)
            .Map(d => d.PublicationLevel, s => s.PublicationLevel)
            .Map(d => d.PublicationLevelProposal, s => s.PublicationLevelProposal)
            .Map(d => d.RegistrationStatus, s => s.RegistrationStatus)
            .Map(d => d.RegistrationStatusProposal, s => s.RegistrationStatusProposal)
            .Ignore(d => d.Relation)
            .Ignore(d => d.Requires)
            .Map(d => d.ResponsibleDeputy, s => s.ResponsibleDeputy)
            .Map(d => d.ResponsiblePerson, s => s.ResponsiblePerson)
            .Map(d => d.Sectors, s => s.Sectors)
            .Map(d => d.Spatial, s => s.Spatial)
            .Map(d => d.SpatialCH, s => s.SpatialCH)
            .Map(d => d.System, s => s.System)
            .Map(d => d.ThematicAreas, s => s.ThematicAreas)
            .Map(d => d.Title, s => s.Name);

        config.NewConfig<PublicServiceModel, IdLabel>()
            .Map(d => d.Id, s => s.Id)
            .Map(d => d.Label, s => s.Name);
    }
}