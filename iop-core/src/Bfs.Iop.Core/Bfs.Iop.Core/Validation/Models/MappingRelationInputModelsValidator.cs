using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class MappingRelationInputModelsValidator : AbstractValidator<IEnumerable<MappingRelationInputModel>>
{
    public MappingRelationInputModelsValidator(IValidator<MappingRelationInputModel> validator)
    {
        RuleForEach(x => x)
            .SetValidator(validator);
    }
}
