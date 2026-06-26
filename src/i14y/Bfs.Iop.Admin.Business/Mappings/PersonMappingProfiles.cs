using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PersonMappingProfiles : Profile
{
    public PersonMappingProfiles()
        : base(nameof(PersonMappingProfiles))
    {
        CreateMap<IopPersonModel, Models.Person>()
            .ForMember(t => t.LastName, opt => opt.MapFrom(s => s.FamilyName))
            .ForMember(t => t.FirstName, opt => opt.MapFrom(s => s.GivenName))
            .ForMember(t => t.Identifier, opt => opt.MapFrom(s => s.Email))
            .ForMember(t => t.Name, opt => opt.MapFrom(s => $"{s.GivenName} {s.FamilyName}"))
            .ForMember(t => t.Id, opt => opt.Ignore())
            ;

        CreateMap<Models.Person, IopPersonModel>()
            .ForMember(t => t.Email, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(t => t.GivenName, opt => opt.MapFrom(s => s.FirstName))
            .ForMember(t => t.FamilyName, opt => opt.MapFrom(s => s.LastName))
            ;

        CreateMap<Models.Person, EmailInputModel>()
            .ForMember(dst => dst.Email, options => options.MapFrom(src => src.Identifier));

        CreateMap<IopPersonModel, EmailInputModel>()
            ;
    }
}