using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DataServiceInputMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DataServiceModel, DataServiceInput>()
            .Map(dest => dest.AccessRightCode, src => src.AccessRights.Code)
            .Map(dest => dest.ConformTos, src => src.ConformsTo)
            .Map(dest => dest.ContactPoints, src => src.ContactPoints)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Documents, src => src.Documentation)
            .Map(dest => dest.EndpointDescriptions, src => src.EndpointDescriptions)
            .Map(dest => dest.EndpointUrls, src => src.EndpointUrls)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Issued, src => src.Issued)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.LandingPages, src => src.LandingPages)
            .Map(dest => dest.License, src => src.License)
            .Map(dest => dest.Modified, src => src.Modified)
            .Map(dest => dest.PreviousVersion, src => src.PreviousVersion)
            .Map(dest => dest.Publisher, src => src.Publisher)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.ServesDatasets, src => src.ServesDatasets)
            .Map(dest => dest.ThemeCodes, src => src.Themes.Select(x => x.Code))
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.VersionNotes, src => src.VersionNotes);

        config.NewConfig<DataServiceInput, DataServiceInputModel>()
            .Map(dest => dest.AccessRights, src => new CodeInputModel
            {
                Code = src.AccessRightCode
            })
            .Map(dest => dest.ConformsTo, src => src.ConformTos)
            .Map(dest => dest.ContactPoints, src => src.ContactPoints)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Documentation, src => src.Documents)
            .Map(dest => dest.EndpointDescriptions, src => src.EndpointDescriptions)
            .Map(dest => dest.EndpointUrls, src => src.EndpointUrls)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Issued, src => src.Issued)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.LandingPages, src => src.LandingPages)
            .Map(dest => dest.License, src => src.License)
            .Map(dest => dest.Modified, src => src.Modified)
            .Map(dest => dest.PreviousVersion, src => src.PreviousVersion)
            .Map(dest => dest.Publisher, src => src.Publisher)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.ServesDatasets, src => src.ServesDatasets)
            .Map(dest => dest.Themes, src => src.ThemeCodes.Select(code => new CodeInputModel
            {
                Code = code
            }))
            .Map(dest => dest.Title, src => src.Title)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.VersionNotes, src => src.VersionNotes);
    }
}