using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CodeListEntryModelMappingProfiles : Profile
{
    public CodeListEntryModelMappingProfiles() : base(nameof(CodeListEntryModelMappingProfiles))
    {
        CreateMap<CodelistEntryInput, CodeListEntryInputModel>()
            .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Value))
            .ForMember(dest => dest.ParentCode, opt => opt.MapFrom(src => src.ParentCode))
            .ForMember(dest => dest.Annotations, opt => opt.Ignore())
            .ForMember(d => d.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(d => d.ValidTo, opt => opt.MapFrom(s => s.ValidTo))
        ;

        CreateMap<CodeListEntryModel, CodeListEntryDetail>()
            .ForMember(dest => dest.Annotations, opt => opt.MapFrom(src => src.Annotations))
            .ForMember(dest => dest.HasChildren, opt => opt.Ignore()) //Needed for tree view in Public FE
            .ForMember(dest => dest.CodelistId, opt => opt.Ignore()) // Not needed anymore
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ParentCode, opt => opt.MapFrom(src => src.ParentCode))
            .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Code))
            .ForMember(dest => dest.ConceptId, opt => opt.Ignore())
            .ForMember(d => d.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(d => d.ValidTo, opt => opt.MapFrom(s => s.ValidTo))
            ;

        CreateMap<CodeListEntryModel, CodeListEntryInputModel>()
            .ForMember(d => d.Annotations, opt => opt.MapFrom(s => s.Annotations))
            .ForMember(d => d.Code, opt => opt.MapFrom(s => s.Code))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.ParentCode, opt => opt.MapFrom(s => s.ParentCode))
            .ForMember(d => d.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(d => d.ValidTo, opt => opt.MapFrom(s => s.ValidTo));

        CreateMap<CodeListEntryModel, CodelistEntryInput>()
            .ForMember(d => d.CodelistId, opt => opt.Ignore())
            .ForMember(d => d.ConceptId, opt => opt.Ignore())
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.ParentCode, opt => opt.MapFrom(s => s.ParentCode))
            .ForMember(d => d.Value, opt => opt.MapFrom(s => s.Code))
            .ForMember(d => d.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(d => d.ValidTo, opt => opt.MapFrom(s => s.ValidTo));
    }
}
