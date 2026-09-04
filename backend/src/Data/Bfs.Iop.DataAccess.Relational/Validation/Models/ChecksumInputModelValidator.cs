using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class ChecksumInputModelValidator : AbstractValidator<ChecksumInputModel>
{
    public ChecksumInputModelValidator(VocabularyEntryCodeValidator<ChecksumAlgorithmsVocabulary> algorithmsValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Algorithm.Code)
            .SetValidator(algorithmsValidator);

        RuleFor(x => x.ChecksumValue)
            .NotEmpty();
    }
}
