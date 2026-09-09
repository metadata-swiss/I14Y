using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class MappingRelationInputModelsValidator : AbstractValidator<IEnumerable<MappingRelationInputModel>>
{
    public MappingRelationInputModelsValidator(IValidator<MappingRelationInputModel> validator)
    {
        RuleForEach(x => x)
            .SetValidator(validator);
    }
}
