using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class IopConceptMappingProfiles : Profile
{
    public IopConceptMappingProfiles() : base(nameof(IopConceptMappingProfiles))
    {
        CreateMap<IopConceptModel, IopConceptInputModel>()
            .ForMember(d => d.Replaces, o => o.MapFrom(s =>
                s.Replaces.Where(r => r.ConceptId.HasValue).Select(r => new IdModel { Id = r.ConceptId!.Value })))
            ;

        CreateMap<ConceptInputCreateVersion, IopConceptInputModel>()
            .ForMember(dest => dest.CodeListEntryValueType, opt => opt.Ignore())
            .ForMember(dest => dest.CodeListEntryValueMaxLength, opt => opt.Ignore())
            .ForMember(dest => dest.CodeListEntryDefaultSortProperty, opt => opt.Ignore())
            .ForMember(dest => dest.ConformsTo, opt => opt.Ignore())
            .ForMember(dest => dest.Replaces, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptType, opt => opt.Ignore())
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Identifiers, opt => opt.MapFrom(src => src.Identifiers))
            .ForMember(dest => dest.Keywords, opt => opt.Ignore())
            .ForMember(dest => dest.MaxLength, opt => opt.Ignore())
            .ForMember(dest => dest.MaxValue, opt => opt.Ignore())
            .ForMember(dest => dest.MeasurementUnit, opt => opt.Ignore())
            .ForMember(dest => dest.MinLength, opt => opt.Ignore())
            .ForMember(dest => dest.MinValue, opt => opt.Ignore())
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NumberDecimals, opt => opt.Ignore())
            .ForMember(dest => dest.Pattern, opt => opt.Ignore())
            .ForMember(dest => dest.Publisher, opt => opt.Ignore())
            .ForMember(dest => dest.ResponsibleDeputy, opt => opt.MapFrom(src => src.ResponsibleDeputy != null ? new EmailInputModel() { Email = src.ResponsibleDeputy.Identifier } : null))
            .ForMember(dest => dest.ResponsiblePerson, opt => opt.MapFrom(src => new EmailInputModel() { Email = src.ResponsiblePerson.Identifier }))
            .ForMember(dest => dest.Themes, opt => opt.Ignore())
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version));
        ;

        CreateMap<IopConceptModel, ConceptVersionView>()
            .ForMember(dest => dest.ConceptId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.ConceptType, opt => opt.MapFrom(src => src.ConceptType))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.RegistrationStatus, opt => opt.MapFrom(src => src.RegistrationStatus))
            .ForMember(dest => dest.PublicationLevel, opt => opt.MapFrom(src => src.PublicationLevel));

        CreateMap<IopConceptModel, ConceptView>()
            .ForMember(dest => dest.Publisher, opt => opt.MapFrom(src => src.Publisher))
            .ForMember(dest => dest.CodelistEntryValueMaxLength, opt => opt.MapFrom(src => src.CodeListEntryValueMaxLength))
            .ForMember(dest => dest.CodeListEntryValueType, opt => opt.MapFrom(src => src.CodeListEntryValueType))
            .ForMember(dest => dest.CodeListEntryDefaultSortProperty, opt => opt.MapFrom(s => s.CodeListEntryDefaultSortProperty))
            .ForMember(dest => dest.CodeListId, opt => opt.Ignore())
            .ForMember(dest => dest.ConceptType, opt => opt.MapFrom(src => src.ConceptType))
            .ForMember(dest => dest.ConformsTo, opt => opt.MapFrom(src => src.ConformsTo))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dest => dest.Identifiers, opt => opt.MapFrom(src => src.Identifiers))
            .ForMember(dest => dest.IsLocked, opt => opt.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.Keywords, opt => opt.MapFrom(src => src.Keywords))
            .ForMember(dest => dest.MaxLength, opt => opt.MapFrom(src => src.MaxLength))
            .ForMember(dest => dest.MaxValue, opt => opt.MapFrom(src => src.MaxValue))
            .ForMember(dest => dest.MeasurementUnit, opt => opt.MapFrom(s => s.MeasurementUnit))
            .ForMember(dest => dest.MinLength, opt => opt.MapFrom(src => src.MinLength))
            .ForMember(dest => dest.MinValue, opt => opt.MapFrom(src => src.MinValue))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
            .ForMember(dest => dest.NbDecimal, opt => opt.MapFrom(src => src.NumberDecimals))
            .ForMember(dest => dest.Pattern, opt => opt.MapFrom(src => src.Pattern))
            .ForMember(dest => dest.Replaces, opt => opt.MapFrom(src => src.Replaces))
            .ForMember(dest => dest.IsReplacedBy, opt => opt.MapFrom(src => src.IsReplacedBy))
            .ForMember(dest => dest.ResponsibleDeputy, opt => opt.MapFrom(src => src.ResponsibleDeputy))
            .ForMember(dest => dest.ResponsiblePerson, opt => opt.MapFrom(src => src.ResponsiblePerson))
            .ForMember(dest => dest.System, opt => opt.MapFrom(src => src.System))
            .ForMember(dest => dest.Themes, opt => opt.MapFrom(src => src.Themes))
            .ForMember(dest => dest.ValidFrom, opt => opt.MapFrom(src => src.ValidFrom))
            .ForMember(dest => dest.ValidTo, opt => opt.MapFrom(src => src.ValidTo))
            .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version));

        CreateMap<IopConceptInputModel, ConceptInput>()
            .ForMember(d => d.Publisher, opt => opt.MapFrom(s => s.Publisher))
            .ForMember(d => d.CodelistEntryValueMaxLength, opt => opt.MapFrom(s => s.CodeListEntryValueMaxLength))
            .ForMember(d => d.CodeListEntryValueType, opt => opt.MapFrom(s => s.CodeListEntryValueType))
            .ForMember(d => d.ConceptType, opt => opt.MapFrom(s => s.ConceptType))
            .ForMember(d => d.ConformsTo, opt => opt.MapFrom(s => s.ConformsTo))
            .ForMember(d => d.CodeListEntryDefaultSortProperty, opt => opt.MapFrom(s => s.CodeListEntryDefaultSortProperty))
            .ForMember(d => d.Description, opt => opt.MapFrom(s => s.Description))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Identifiers, opt => opt.MapFrom(s => s.Identifiers))
            .ForMember(d => d.Keywords, opt => opt.MapFrom(s => s.Keywords))
            .ForMember(d => d.MaxLength, opt => opt.MapFrom(s => s.MaxLength))
            .ForMember(d => d.MaxValue, opt => opt.MapFrom(s => s.MaxValue))
            .ForMember(d => d.MeasurementUnit, opt => opt.MapFrom(s => s.MeasurementUnit))
            .ForMember(d => d.MinLength, opt => opt.MapFrom(s => s.MinLength))
            .ForMember(d => d.MinValue, opt => opt.MapFrom(s => s.MinValue))
            .ForMember(d => d.Name, opt => opt.MapFrom(s => s.Name))
            .ForMember(d => d.NbDecimal, opt => opt.MapFrom(s => s.NumberDecimals))
            .ForMember(d => d.Pattern, opt => opt.MapFrom(s => s.Pattern))
            .ForMember(d => d.Replaces, opt => opt.MapFrom(s => s.Replaces))
            .ForMember(d => d.ResponsibleDeputy, opt => opt.Ignore())
            .ForMember(d => d.ResponsiblePerson, opt => opt.Ignore())
            .ForMember(d => d.ThemeCodes, opt => opt.MapFrom(s => s.Themes.Select(x => x.Code)))
            .ForMember(d => d.ValidFrom, opt => opt.MapFrom(s => s.ValidFrom))
            .ForMember(d => d.ValidTo, opt => opt.MapFrom(s => s.ValidTo))
            .ForMember(d => d.Version, opt => opt.MapFrom(s => s.Version));
    }
}
