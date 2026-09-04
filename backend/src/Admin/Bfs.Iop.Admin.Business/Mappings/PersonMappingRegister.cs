using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PersonMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IopPersonModel, Models.Person>()
            .Map(dest => dest.LastName, src => src.FamilyName)
            .Map(dest => dest.FirstName, src => src.GivenName)
            .Map(dest => dest.Identifier, src => src.Email)
            .Map(dest => dest.Name, src => $"{src.GivenName} {src.FamilyName}")
            .Ignore(dest => dest.Id);

        config.NewConfig<Models.Person, IopPersonModel>()
            .Map(dest => dest.Email, src => src.Identifier)
            .Map(dest => dest.GivenName, src => src.FirstName)
            .Map(dest => dest.FamilyName, src => src.LastName);

        config.NewConfig<Models.Person, EmailInputModel>()
            .Map(dest => dest.Email, src => src.Identifier);

        config.NewConfig<IopPersonModel, EmailInputModel>();
    }
}