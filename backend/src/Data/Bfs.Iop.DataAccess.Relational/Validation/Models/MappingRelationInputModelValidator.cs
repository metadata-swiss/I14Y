using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

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
