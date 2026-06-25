using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DataServiceInputMappingProfiles : Profile
{
    public DataServiceInputMappingProfiles()
        : base(nameof(DataServiceInputMappingProfiles))
    {
        CreateMap<DataServiceModel, DataServiceInput>()
            .ForMember(d => d.AccessRightCode, opt => opt.MapFrom(s => s.AccessRights.Code))
            .ForMember(d => d.ConformTos, opt => opt.MapFrom(s => s.ConformsTo))
            .ForMember(d => d.ContactPoints, opt => opt.MapFrom(s => s.ContactPoints))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documents, opt => opt.MapFrom(s => s.Documentation))
            .ForMember(d => d.EndpointDescriptions, opt => opt.MapFrom(s => s.EndpointDescriptions))
            .ForMember(d => d.EndpointUrls, opt => opt.MapFrom(s => s.EndpointUrls))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Issued, opt => opt.MapFrom(s => s.Issued))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.LandingPages, opt => opt.MapFrom(s => s.LandingPages))
            .ForMember(d => d.License, opt => opt.MapFrom(s => s.License))
            .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.Modified))
            .ForMember(d => d.PreviousVersion, opt => opt.MapFrom(s => s.PreviousVersion))
            .ForMember(d => d.Publisher, opt => opt.MapFrom(s => s.Publisher))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.ServesDatasets, opt => opt.MapFrom(s => s.ServesDatasets))
            .ForMember(d => d.ThemeCodes, opt => opt.MapFrom(s => s.Themes.Select(x => x.Code)))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
            .ForMember(d => d.VersionNotes, opt => opt.MapFrom(s => s.VersionNotes));

        CreateMap<DataServiceInput, DataServiceInputModel>()
            .ForMember(d => d.AccessRights, opt => opt.MapFrom(s => new CodeInputModel() { Code = s.AccessRightCode }))
            .ForMember(d => d.ConformsTo, opt => opt.MapFrom(s => s.ConformTos))
            .ForMember(d => d.ContactPoints, opt => opt.MapFrom(s => s.ContactPoints))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Documentation, opt => opt.MapFrom(s => s.Documents))
            .ForMember(d => d.EndpointDescriptions, opt => opt.MapFrom(s => s.EndpointDescriptions))
            .ForMember(d => d.EndpointUrls, opt => opt.MapFrom(s => s.EndpointUrls))
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Issued, opt => opt.MapFrom(s => s.Issued))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.LandingPages, opt => opt.MapFrom(s => s.LandingPages))
            .ForMember(d => d.License, opt => opt.MapFrom(s => s.License))
            .ForMember(d => d.Modified, opt => opt.MapFrom(s => s.Modified))
            .ForMember(d => d.PreviousVersion, opt => opt.MapFrom(s => s.PreviousVersion))
            .ForMember(d => d.Publisher, opt => opt.MapFrom(s => s.Publisher))
            .ForMember(d => d.ResponsiblePerson, opt => opt.MapFrom(s => s.ResponsiblePerson))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.MapFrom(s => s.ResponsibleDeputy))
            .ForMember(d => d.ServesDatasets, opt => opt.MapFrom(s => s.ServesDatasets))
            .ForMember(d => d.Themes, opt => opt.MapFrom(s => s.ThemeCodes.Select(code => new CodeInputModel() { Code = code })))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version))
            .ForMember(d => d.VersionNotes, opt => opt.MapFrom(s => s.VersionNotes));
    }
}