using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class AnnotationInputModelsValidator : AbstractValidator<IEnumerable<AnnotationInputModel>>
{
    public AnnotationInputModelsValidator(IValidator<AnnotationInputModel> annotationInputModelValidator) => 
        RuleForEach(x => x).SetValidator(annotationInputModelValidator);
}
