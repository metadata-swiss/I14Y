using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class AnnotationInputModelsValidator : AbstractValidator<IEnumerable<AnnotationInputModel>>
{
    public AnnotationInputModelsValidator(IValidator<AnnotationInputModel> annotationInputModelValidator) => 
        RuleForEach(x => x).SetValidator(annotationInputModelValidator);
}
