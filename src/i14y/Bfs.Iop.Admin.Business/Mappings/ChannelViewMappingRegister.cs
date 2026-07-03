using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ChannelViewMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ChannelModel, ChannelView>()
            .Map(dest => dest.Address, src => src.Address)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Email, src => src.Email)
            .Map(dest => dest.Fax, src => src.Fax)
            .Ignore(dest => dest.Id)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.Mobile, src => src.Mobile)
            .Map(dest => dest.OpeningHours, src => src.OpeningHours)
            .Map(dest => dest.OwnedBy, src => src.OwnedBy)
            .Map(dest => dest.Phone, src => src.Phone)
            .Map(dest => dest.Type, src => src.Type)
            .Map(dest => dest.Url, src => src.Url);

        config.NewConfig<ChannelModel, ChannelSummary>()
            .Ignore(dest => dest.Id)
            .Map(dest => dest.Identifier, src => src.Identifier)
            .Map(dest => dest.OwnedBy, src => src.OwnedBy.Select(x => x.Name))
            .Map(
                dest => dest.Type,
                src => src.Type!.Name,
                src => src.Type != null);
    }
}