using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

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
