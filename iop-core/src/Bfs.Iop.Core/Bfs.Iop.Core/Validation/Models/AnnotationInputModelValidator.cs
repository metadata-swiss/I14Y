using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class AnnotationInputModelValidator : AbstractValidator<AnnotationInputModel>
{
    public AnnotationInputModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(model => model.Identifier)
            .Must(x => !x.IsNullOrEmptyOrContainsWhiteSpace())
            .When(model => model.Identifier is not null)
            .WithMessage(_ => "The value cannot be empty or contain white spaces.");

        RuleFor(model => model.Text!)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace()
            .When(model => model.Text is not null);

        RuleFor(model => model.Title)
            .Must(x => !string.IsNullOrWhiteSpace(x))
            .When(model => model.Title is not null);

        RuleFor(model => model.Type)
            .Must(type => !type.IsNullOrEmptyOrContainsWhiteSpace())
            .WithMessage("The value cannot be null, empty or contain white spaces.");

        RuleFor(model => model.Uri)
            .Must(uri => uri!.IsValidUri())
            .When(model => model.Uri is not null)
            .WithMessage("The value must be valid Uri.");
    }
}
