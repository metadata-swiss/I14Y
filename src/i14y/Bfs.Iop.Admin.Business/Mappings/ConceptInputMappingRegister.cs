using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ConceptInputMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ConceptView, ConceptInput>()
            .Ignore(dest => dest.ThemeCodes)
            .Map(dest => dest.Replaces, src =>
                src.Replaces
                    .Where(r => r.ConceptId.HasValue)
                    .Select(r => new IdModel
                    {
                        Id = r.ConceptId!.Value
                    }));

        config.NewConfig<ConceptInput, IopConceptInputModel>()
            .Map(dest => dest.CodeListEntryValueMaxLength, src => src.CodelistEntryValueMaxLength)
            .Map(dest => dest.CodeListEntryDefaultSortProperty, src => src.CodeListEntryDefaultSortProperty)
            .Map(dest => dest.NumberDecimals, src => src.NbDecimal)
            .Map(dest => dest.Publisher, src => src.Publisher)
            .Map(dest => dest.ResponsibleDeputy, src => src.ResponsibleDeputy)
            .Map(dest => dest.ResponsiblePerson, src => src.ResponsiblePerson)
            .Map(
                dest => dest.Themes,
                src => src.ThemeCodes!.Select(x => new CodeInputModel
                {
                    Code = x
                }),
                src => src.ThemeCodes != null)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo);

        config.NewConfig<IopConceptModel, ConceptInput>()
            .Map(dest => dest.CodeListEntryDefaultSortProperty, src => src.CodeListEntryDefaultSortProperty)
            .Map(dest => dest.ValidFrom, src => src.ValidFrom)
            .Map(dest => dest.ValidTo, src => src.ValidTo)
            .Map(dest => dest.NbDecimal, src => src.NumberDecimals)
            .Map(dest => dest.ThemeCodes, src => src.Themes.Select(i => i.Code))
            .Map(dest => dest.MeasurementUnit, src => src.MeasurementUnit)
            .Map(dest => dest.Replaces, src =>
                src.Replaces
                    .Where(r => r.ConceptId.HasValue)
                    .Select(r => new IdModel
                    {
                        Id = r.ConceptId!.Value
                    }));
    }
}