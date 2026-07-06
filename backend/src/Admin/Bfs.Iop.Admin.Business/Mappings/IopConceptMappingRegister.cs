using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class IopConceptMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<IopConceptModel, IopConceptInputModel>()
            .Map(dest => dest.Replaces, src =>
                src.Replaces
                    .Where(r => r.ConceptId.HasValue)
                    .Select(r => new IdModel
                    {
                        Id = r.ConceptId!.Value
                    }));

        config.NewConfig<ConceptInputCreateVersion, IopConceptInputModel>()
            .Ignore(dest => dest.CodeListEntryValueType)
            .Ignore(dest => dest.CodeListEntryValueMaxLength)
            .Ignore(dest => dest.CodeListEntryDefaultSortProperty)
            .Ignore(dest => dest.ConformsTo)
            .Ignore(dest => dest.Replaces)
            .Ignore(dest => dest.ConceptType)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Ignore(dest => dest.Keywords)
            .Ignore(dest => dest.MaxLength)
            .Ignore(dest => dest.MaxValue)
            .Ignore(dest => dest.MeasurementUnit)
            .Ignore(dest => dest.MinLength)
            .Ignore(dest => dest.MinValue)
            .Map(dest => dest.Name, src => src.Name)
            .Ignore(dest => dest.NumberDecimals)
            .Ignore(dest => dest.Pattern)
            .Ignore(dest => dest.Publisher)
            .Map(
                dest => dest.ResponsibleDeputy,
                src => src.ResponsibleDeputy != null
                    ? new EmailInputModel { Email = src.ResponsibleDeputy.Identifier }
                    : null)
            .Map(
                dest => dest.ResponsiblePerson,
                src => new EmailInputModel { Email = src.ResponsiblePerson.Identifier })
            .Ignore(dest => dest.Themes)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.Version, src => src.Version);

        config.NewConfig<IopConceptModel, ConceptVersionView>()
            .Map(dest => dest.ConceptId, src => src.Id)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.ConceptType, src => src.ConceptType)
            .Map(dest => dest.Version, src => src.Version)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.RegistrationStatus, src => src.RegistrationStatus)
            .Map(dest => dest.PublicationLevel, src => src.PublicationLevel);

        config.NewConfig<IopConceptModel, ConceptView>()
            .Map(dest => dest.Publisher, src => src.Publisher)
            .Map(dest => dest.CodelistEntryValueMaxLength, src => src.CodeListEntryValueMaxLength)
            .Map(dest => dest.CodeListEntryValueType, src => src.CodeListEntryValueType)
            .Map(dest => dest.CodeListEntryDefaultSortProperty, src => src.CodeListEntryDefaultSortProperty)
            .Ignore(dest => dest.CodeListId)
            .Map(dest => dest.ConceptType, src => src.ConceptType)
            .Map(dest => dest.ConformsTo, src => src.ConformsTo)
            .Map(dest => dest.Description, src => src.Description)
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.IsLocked, src => src.IsLocked)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.MaxLength, src => src.MaxLength)
            .Map(dest => dest.MaxValue, src => src.MaxValue)
            .Map(dest => dest.MeasurementUnit, src => src.MeasurementUnit)
            .Map(dest => dest.MinLength, src => src.MinLength)
            .Map(dest => dest.MinValue, src => src.MinValue)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.NbDecimal, src => src.NumberDecimals)
            .Map(dest => dest.Pattern, src => src.Pattern)
            .Map(dest => dest.Replaces, src => src.Replaces)
            .Map(dest => dest.IsReplacedBy, src => src.IsReplacedBy)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(dest => dest.System, src => src.System)
            .Map(dest => dest.Themes, src => src.Themes)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.Version, src => src.Version);

        config.NewConfig<IopConceptInputModel, ConceptInput>()
            .Map(dest => dest.Publisher, src => src.Publisher)
            .Map(dest => dest.CodelistEntryValueMaxLength, src => src.CodeListEntryValueMaxLength)
            .Map(dest => dest.CodeListEntryValueType, src => src.CodeListEntryValueType)
            .Map(dest => dest.ConceptType, src => src.ConceptType)
            .Map(dest => dest.ConformsTo, src => src.ConformsTo)
            .Map(dest => dest.CodeListEntryDefaultSortProperty, src => src.CodeListEntryDefaultSortProperty)
            .Map(dest => dest.Description, src => src.Description)
            .Ignore(dest => dest.Id)
            .Map(dest => dest.Identifiers, src => src.Identifiers)
            .Map(dest => dest.Keywords, src => src.Keywords)
            .Map(dest => dest.MaxLength, src => src.MaxLength)
            .Map(dest => dest.MaxValue, src => src.MaxValue)
            .Map(dest => dest.MeasurementUnit, src => src.MeasurementUnit)
            .Map(dest => dest.MinLength, src => src.MinLength)
            .Map(dest => dest.MinValue, src => src.MinValue)
            .Map(dest => dest.Name, src => src.Name)
            .Map(dest => dest.NbDecimal, src => src.NumberDecimals)
            .Map(dest => dest.Pattern, src => src.Pattern)
            .Map(dest => dest.Replaces, src => src.Replaces)
            .Ignore(dest => dest.ResponsibleDeputy)
            .Ignore(dest => dest.ResponsiblePerson)
            .Map(dest => dest.ThemeCodes, src => src.Themes.Select(x => x.Code))
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.Version, src => src.Version);
    }
}