using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class MappingRelationInputModelValidator : AbstractValidator<MappingRelationInputModel>
{
    public MappingRelationInputModelValidator(
        VocabularyEntryCodeValidator<MappingPredicateVocabulary> relationTypeValidator)
    {
        RuleFor(x => x.Source.Uri)
            .Must(uri => uri.IsValidUri())
            .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");

        RuleFor(x => x.Target.Uri)
            .Must(uri => uri.IsValidUri())
            .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");

        RuleFor(x => x.RelationType.Code)
            .SetValidator(relationTypeValidator);
    }
}
