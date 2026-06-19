using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class KeywordInputModelValidator : AbstractValidator<KeywordModel>
{
    public KeywordInputModelValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(_ => _)
            .Must(x => !(x.Label is null && x.Uri is null))
            .WithMessage("Keyword must contain a label or an uri.");

        RuleFor(x => x.Label!)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace()
            .When(x => x.Label is not null);

        RuleFor(x => x.Uri)
           .Must(uri => uri!.IsValidUri())
           .When(x => x.Uri is not null)
           .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");
    }
}
