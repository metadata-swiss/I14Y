using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ChannelInputMappingProfiles : Profile
{
    public ChannelInputMappingProfiles()
        : base(nameof(ChannelViewMappingProfiles))
    {
        CreateMap<Models.ChannelInput, ChannelInputModel>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
            .ForMember(d => d.Fax, opt => opt.MapFrom(s => s.Fax))
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Mobile, opt => opt.MapFrom(s => s.Mobile))
            .ForMember(d => d.OpeningHours, opt => opt.MapFrom(s => s.OpeningHours))
            .ForMember(d => d.OwnedBy, opt => opt.Ignore())
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.Url, opt => opt.MapFrom(s => s.Url));

        CreateMap<ChannelModel, ChannelInputModel>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
            .ForMember(d => d.Fax, opt => opt.MapFrom(s => s.Fax))
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Mobile, opt => opt.MapFrom(s => s.Mobile))
            .ForMember(d => d.OpeningHours, opt => opt.MapFrom(s => s.OpeningHours))
            .ForMember(d => d.OwnedBy, opt => opt.MapFrom(s => s.OwnedBy.Select(x => new IdentifierInputModel() { Identifier = x.Identifier })))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.Url, opt => opt.MapFrom(s => s.Url));
    }
}