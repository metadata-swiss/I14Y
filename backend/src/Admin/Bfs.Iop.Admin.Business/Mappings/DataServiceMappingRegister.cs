using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DataServiceMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DataServiceModel, DataService>()
            .Map(dest => dest.AccessRights, src => src.AccessRights)
            .Map(dest => dest.ConformTos, src => src.ConformsTo)
            .Map(dest => dest.ContactPoints, src => src.ContactPoints)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Documents, src => src.Documentation)
            .Map(dest => dest.EndpointDescriptions, src => src.EndpointDescriptions)
            .Map(dest => dest.EndpointUrls, src => src.EndpointUrls)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Issued, src => src.Issued)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.LandingPages, src => src.LandingPages)
            .Map(dest => dest.License, src => src.License)
            .Map(dest => dest.Modified, src => src.Modified)
            .Ignore(dest => dest.NextVersions)
            .Ignore(dest => dest.PreviousVersion)
            .Map(dest => dest.PublicationLevel, src => src.PublicationLevel)
            .Map(dest => dest.PublicationLevelProposal, src => src.PublicationLevelProposal)
            .Map(dest => dest.PublisherName, src => src.Publisher.Name)
            .Map(dest => dest.RegistrationStatus, src => src.RegistrationStatus)
            .Map(dest => dest.RegistrationStatusProposal, src => src.RegistrationStatusProposal)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Ignore(dest => dest.ServesDatasets)
            .Ignore(dest => dest.Status)
            .Map(dest => dest.System, src => src.System)
            .Map(dest => dest.Themes, src => src.Themes)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.VersionNotes, src => src.VersionNotes);

        config.NewConfig<DataServiceModel, DataServiceVersionSummary>()
            .Map(dest => dest.EndpointUrls, src => src.EndpointUrls)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version);
    }
}