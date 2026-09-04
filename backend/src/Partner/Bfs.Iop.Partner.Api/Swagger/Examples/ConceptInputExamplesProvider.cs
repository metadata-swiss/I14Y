using Bfs.Iop.Common.Extensions;
using Bfs.Iop.Common.Serialization.Json;
using Bfs.Iop.DataAccess.Abstractions;
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
