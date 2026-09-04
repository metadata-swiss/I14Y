using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.Partner.Business.Examples;
using Bfs.Iop.Partner.Models.ConceptsInput;

namespace Bfs.Iop.Partner.Business.UnitTests.Helpers;

internal static class ModelsHelper
{
    public static CodeListConceptInput CodeListConceptInputExample => 
        (CodeListConceptInput)ConceptInputExamples.GetConceptInputExample(ConceptType.CodeList);

    public static DateConceptInput DateConceptInputExample =>
        (DateConceptInput)ConceptInputExamples.GetConceptInputExample(ConceptType.Date);

    public static NumericConceptInput NumericConceptInputExample =>
        (NumericConceptInput)ConceptInputExamples.GetConceptInputExample(ConceptType.Numeric);

    public static StringConceptInput StringConceptInputExample =>
        (StringConceptInput)ConceptInputExamples.GetConceptInputExample(ConceptType.String);
}
