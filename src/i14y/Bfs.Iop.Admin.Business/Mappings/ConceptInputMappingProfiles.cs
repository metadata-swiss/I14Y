using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ConceptInputMappingProfiles : Profile
{
    public ConceptInputMappingProfiles()
        : base(nameof(ConceptInputMappingProfiles))
    {
        CreateMap<ConceptView, ConceptInput>()
            .ForMember(x => x.ThemeCodes, x => x.Ignore())
            ;

        CreateMap<ConceptInput, IopConceptInputModel>()
            .ForMember(dst => dst.CodeListEntryValueMaxLength, options => options.MapFrom(src => src.CodelistEntryValueMaxLength))
            .ForMember(d => d.CodeListEntryDefaultSortProperty, opt => opt.MapFrom(s => s.CodeListEntryDefaultSortProperty))
            .ForMember(dst => dst.NumberDecimals, options => options.MapFrom(src => src.NbDecimal))
            .ForMember(dst => dst.Publisher, options => options.MapFrom(s => s.Publisher))
            .ForMember(dst => dst.ResponsibleDeputy, options => options.MapFrom(src => src.ResponsibleDeputy))
            .ForMember(dst => dst.ResponsiblePerson, options => options.MapFrom(src => src.ResponsiblePerson))
            .ForMember(dst => dst.Themes, options =>
            {
                options.PreCondition(src => src.ThemeCodes is not null);
                options.MapFrom(src => src.ThemeCodes!.Select(x => new CodeInputModel { Code = x }));
            })
            .ForMember(dst => dst.ValidFrom, options => options.MapFrom(src => src.ValidFrom))
            .ForMember(dst => dst.ValidTo, options => options.MapFrom(src => src.ValidTo))
            ;

        CreateMap<IopConceptModel, ConceptInput>()
            .ForMember(d => d.CodeListEntryDefaultSortProperty, opt => opt.MapFrom(s => s.CodeListEntryDefaultSortProperty))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.NbDecimal, opt => opt.MapFrom(src => src.NumberDecimals))
            .ForMember(dest => dest.ThemeCodes, opt => opt.MapFrom(src => src.Themes.Select(i => i.Code)))
            .ForMember(dest => dest.MeasurementUnit, opt => opt.MapFrom(s => s.MeasurementUnit))
        ;
    }
}