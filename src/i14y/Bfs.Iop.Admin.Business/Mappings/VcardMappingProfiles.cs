using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class VcardMappingProfiles : Profile
{
    public VcardMappingProfiles()
        : base(nameof(VcardMappingProfiles))
    {
        CreateMap<Vcard, VCardModel>()
            .ForMember(d => d.HasAddress, opt => opt.MapFrom(s => s.AdrWork))
            .ForMember(d => d.Fn, opt => opt.MapFrom(s => s.Fn))
            .ForMember(d => d.HasEmail, opt => opt.MapFrom(s => s.EmailInternet))
            .ForMember(d => d.HasTelephone, opt => opt.MapFrom(s => s.TelWorkVoice))
            .ForMember(d => d.Kind, opt => opt.MapFrom(_ => VCardKind.Organization))
            .ForMember(d => d.Note, opt => opt.MapFrom(s => s.Note));

        CreateMap<VCardModel, Vcard>()
            .ForMember(d => d.AdrWork, opt => opt.MapFrom(s => s.HasAddress))
            .ForMember(d => d.EmailInternet, opt => opt.MapFrom(s => s.HasEmail))
            .ForMember(d => d.Fn, opt => opt.MapFrom(s => s.Fn))
            .ForMember(d => d.Note, opt => opt.MapFrom(s => s.Note))
            .ForMember(d => d.Org, opt => opt.Ignore())
            .ForMember(d => d.TelWorkVoice, opt => opt.MapFrom(s => s.HasTelephone));
    }
}