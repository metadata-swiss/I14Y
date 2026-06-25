using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Admin.Models.EIAM.SOAP.Response;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ActiveDirectoryUserMappingProfiles : Profile
{
    public ActiveDirectoryUserMappingProfiles()
        : base(nameof(ActiveDirectoryUserMappingProfiles))
    {
        CreateMap<UserReturn, ActiveDirectoryUser>()
            .ForMember(t => t.DisplayName, opt => opt.MapFrom(s => $"{s.Name} {s.FirstName}".Trim()))
            .ForMember(t => t.Firstname, opt => opt.MapFrom(s => s.FirstName))
            .ForMember(t => t.Lastname, opt => opt.MapFrom(s => s.Name))
            .ForMember(t => t.OrgUnitName, opt => opt.Ignore())
            ;

        CreateMap<IopPersonModel, ActiveDirectoryUser>()
            .ForMember(t => t.DisplayName, opt => opt.MapFrom(s => $"{s.GivenName} {s.FamilyName}".Trim()))
            .ForMember(t => t.Firstname, opt => opt.MapFrom(s => s.GivenName))
            .ForMember(t => t.Lastname, opt => opt.MapFrom(s => s.FamilyName))
            .ForMember(t => t.OrgUnitName, opt => opt.Ignore())
            ;
    }
}