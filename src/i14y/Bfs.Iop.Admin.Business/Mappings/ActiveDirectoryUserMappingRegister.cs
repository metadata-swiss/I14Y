using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.EIAM.SOAP.Response;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ActiveDirectoryUserMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<UserReturn, ActiveDirectoryUser>()
            .Map(dest => dest.DisplayName, src => $"{src.Name} {src.FirstName}".Trim())
            .Map(dest => dest.Firstname, src => src.FirstName)
            .Map(dest => dest.Lastname, src => src.Name)
            .Ignore(dest => dest.OrgUnitName);

        config.NewConfig<IopPersonModel, ActiveDirectoryUser>()
            .Map(dest => dest.DisplayName, src => $"{src.GivenName} {src.FamilyName}".Trim())
            .Map(dest => dest.Firstname, src => src.GivenName)
            .Map(dest => dest.Lastname, src => src.FamilyName)
            .Ignore(dest => dest.OrgUnitName);
    }
}