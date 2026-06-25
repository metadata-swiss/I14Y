using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ChannelViewMappingProfiles : Profile
{
    public ChannelViewMappingProfiles()
        : base(nameof(ChannelViewMappingProfiles))
    {
        CreateMap<ChannelModel, Models.ChannelView>()
            .ForMember(d => d.Address, opt => opt.MapFrom(s => s.Address))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Email, opt => opt.MapFrom(s => s.Email))
            .ForMember(d => d.Fax, opt => opt.MapFrom(s => s.Fax))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Mobile, opt => opt.MapFrom(s => s.Mobile))
            .ForMember(d => d.OpeningHours, opt => opt.MapFrom(s => s.OpeningHours))
            .ForMember(d => d.OwnedBy, opt => opt.MapFrom(s => s.OwnedBy))
            .ForMember(d => d.Phone, opt => opt.MapFrom(s => s.Phone))
            .ForMember(d => d.Type, opt => opt.MapFrom(s => s.Type))
            .ForMember(d => d.Url, opt => opt.MapFrom(s => s.Url));

        CreateMap<ChannelModel, ChannelSummary>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.OwnedBy, opt => opt.MapFrom(s => s.OwnedBy.Select(x => x.Name)))
            .ForMember(d => d.Type, opt =>
            {
                opt.PreCondition(s => s.Type is not null);
                opt.MapFrom(s => s.Type!.Name);
            });
    }
}