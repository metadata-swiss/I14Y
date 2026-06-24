using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Common.Utilities;
using Bfs.Iop.Partner.Business.Examples;
using Bfs.Iop.Partner.Models.ConceptsInput;
using Swashbuckle.AspNetCore.Filters;

namespace Bfs.Iop.Partner.Api.Swagger.Examples;

public sealed class ConceptInputExamplesProvider : IMultipleExamplesProvider<DataWrapper<ConceptInputBase>>
{
    public IEnumerable<SwaggerExample<DataWrapper<ConceptInputBase>>> GetExamples()
    {
        var conceptTypes = Enum.GetValues<ConceptType>();

        foreach (var type in conceptTypes)
        {
            yield return new SwaggerExample<DataWrapper<ConceptInputBase>>()
            {
                Name = $"{type} concept example",
                Value = ConceptInputExamples.GetConceptInputExample(type).Wrap()
            };
        }
    }
}
