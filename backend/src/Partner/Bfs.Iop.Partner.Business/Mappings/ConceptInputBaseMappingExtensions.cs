using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Partner.Models.ConceptsInput;

namespace Bfs.Iop.Partner.Business.Mappings;

public static class ConceptInputBaseMappingExtensions
{
    public static IopConceptInputModel MapToIopConceptInputModel(this ConceptInputBase concept)
    {
        ArgumentNullException.ThrowIfNull(concept, nameof(concept));

       return concept switch         
       {
            StringConceptInput stringConcept => stringConcept.MapToIopConceptInputModel(),
            NumericConceptInput numericConcept => numericConcept.MapToIopConceptInputModel(),
            DateConceptInput dateConcept => dateConcept.MapToIopConceptInputModel(),
            CodeListConceptInput codeListConcept => codeListConcept.MapToIopConceptInputModel(),
            _ => throw new NotSupportedException($"The concept type '{concept.GetType().Name}' is not supported.")
        };
    }

    private static IopConceptInputModel MapToIopConceptInputModel(this CodeListConceptInput concept)
    {
        var baseConcept = concept.CreateIopConceptInputModel(ConceptType.CodeList);
        
        return baseConcept with 
        {
            CodeListEntryDefaultSortProperty = concept.CodeListEntryDefaultSortProperty,
            CodeListEntryValueMaxLength = concept.CodeListEntryValueMaxLength,
            CodeListEntryValueType = concept.CodeListEntryValueType
        };
    }

    private static IopConceptInputModel MapToIopConceptInputModel(this DateConceptInput concept)
    {
        var baseConcept = concept.CreateIopConceptInputModel(ConceptType.Date);
        
        return baseConcept with 
        {
            Pattern = concept.Pattern
        };
    }

    private static IopConceptInputModel MapToIopConceptInputModel(this NumericConceptInput concept)
    {
        var baseConcept = concept.CreateIopConceptInputModel(ConceptType.Numeric);

        return baseConcept with 
        {
            MaxValue = concept.MaxValue,
            MinValue = concept.MinValue,
            NumberDecimals = concept.NumberDecimals,
            MeasurementUnit = concept.MeasurementUnit,
            Pattern = concept.Pattern
        };
    }

    private static IopConceptInputModel MapToIopConceptInputModel(this StringConceptInput concept)
    {
        var baseConcept = concept.CreateIopConceptInputModel(ConceptType.String);

        return baseConcept with 
        {
            MaxLength = concept.MaxLength,
            MinLength = concept.MinLength,
            Pattern = concept.Pattern
        };
    }

    private static IopConceptInputModel CreateIopConceptInputModel(this ConceptInputBase concept, ConceptType conceptType) => new()
    {
        ConceptType = conceptType,
        ConformsTo = concept.ConformsTo,
        Description = concept.Description,
        Identifiers = concept.Identifiers,
        Keywords = concept.Keywords,
        Name = concept.Name,
        Publisher = concept.Publisher,
        ResponsibleDeputy = concept.ResponsibleDeputy,
        ResponsiblePerson = concept.ResponsiblePerson,
        Themes = concept.Themes,
        ValidFrom = concept.ValidFrom,
        ValidTo = concept.ValidTo,
        Version = concept.Version
    };
}
