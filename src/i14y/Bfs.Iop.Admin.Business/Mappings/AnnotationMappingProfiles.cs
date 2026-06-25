using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AnnotationMappingProfiles : Profile
{
    public AnnotationMappingProfiles()
        : base(nameof(AnnotationMappingProfiles))
    {
        CreateMap<AnnotationModel, AnnotationInputModel>()
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Text))
            .ForMember(d => d.Uri, opt => opt.MapFrom(s => s.Uri));

        CreateMap<AnnotationModel, Models.Annotation>()
            .ForMember(d => d.Identifier, opt => opt.MapFrom(s => s.Identifier))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Position, opt => opt.Ignore())
            .ForMember(d => d.Text, opt => opt.MapFrom(s => s.Text))
            .ForMember(d => d.Uri, opt => opt.MapFrom(s => s.Uri));

        CreateMap<Annotation, AnnotationInputModel>()
            .ForMember(dest => dest.Identifier, opt => opt.MapFrom(src => src.Identifier))
            .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.Text))
            .ForMember(dest => dest.Title, opt => opt.MapFrom(src => src.Title))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
            .ForMember(dest => dest.Uri, opt => opt.MapFrom(src => src.Uri))
            ;
    }
}