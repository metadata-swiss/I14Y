using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ChannelInputMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.ChannelInput, ChannelInputModel>()
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Fax, src => src.Fax)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Mobile, src => src.Mobile)
            .Map(dest => dest.OpeningHours, src => src.OpeningHours)
            .Ignore(dest => dest.OwnedBy)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Url, src => src.Url);

        config.NewConfig<ChannelModel, ChannelInputModel>()
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Fax, src => src.Fax)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Mobile, src => src.Mobile)
            .Map(dest => dest.OpeningHours, src => src.OpeningHours)
            .Map(dest => dest.OwnedBy, src => src.OwnedBy.Select(x => new IdentifierInputModel
            {
                Identifier = x.Identifier
            }))
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Url, src => src.Url);
    }
}