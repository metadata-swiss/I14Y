using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DataServiceMappingProfiles : Profile
{
    public DataServiceMappingProfiles()
        : base(nameof(DataServiceMappingProfiles))
    {
        CreateMap<DataServiceModel, DataService>()
            .ForMember(d => d.AccessRights, opt => opt.MapFrom(s => s.AccessRights))
            .ForMember(d => d.ConformTos, opt => opt.MapFrom(s => s.ConformsTo))
            .ForMember(d => d.ContactPoints, opt => opt.MapFrom(s => s.ContactPoints))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documentation))
            .ForMember(d => d.EndpointDescriptions, opt => opt.MapFrom(s => s.EndpointDescriptions))
            .ForMember(d => d.EndpointUrls, opt => opt.MapFrom(s => s.EndpointUrls))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Issued, opt => opt.MapFrom(s => s.Issued))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.LandingPages, opt => opt.MapFrom(s => s.LandingPages))
            .ForMember(d => d.License, opt => opt.MapFrom(s => s.License))
            .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.Modified))
            .ForMember(d => d.NextVersions, opt => opt.Ignore())
            .ForMember(d => d.PreviousVersion, opt => opt.Ignore())
            .ForMember(d => d.PublicationLevel, opt => opt.MapFrom(s => s.PublicationLevel))
            .ForMember(d => d.PublicationLevelProposal, opt => opt.MapFrom(s => s.PublicationLevelProposal))
            .ForMember(d => d.PublisherName, opt => opt.MapFrom(s => s.Publisher.Name))
            .ForMember(d => d.RegistrationStatus, opt => opt.MapFrom(s => s.RegistrationStatus))
            .ForMember(d => d.RegistrationStatusProposal, opt => opt.MapFrom(s => s.RegistrationStatusProposal))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.ServesDatasets, opt => opt.Ignore())
            .ForMember(d => d.Status, opt => opt.Ignore())
            .ForMember(d => d.System, opt => opt.MapFrom(s => s.System))
            .ForMember(d => d.Themes, opt => opt.MapFrom(s => s.Themes))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
            .ForMember(d => d.VersionNotes, opt => opt.MapFrom(s => s.VersionNotes));

        CreateMap<DataServiceModel, DataServiceVersionSummary>()
            .ForMember(d => d.EndpointUrls, opt => opt.MapFrom(s => s.EndpointUrls))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version));
    }
}