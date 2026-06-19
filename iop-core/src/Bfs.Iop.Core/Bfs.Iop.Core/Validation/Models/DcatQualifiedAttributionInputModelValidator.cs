using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatQualifiedAttributionInputModelValidator : AbstractValidator<DcatQualifiedAttributionInputModel>
{
    public DcatQualifiedAttributionInputModelValidator(
        VocabularyEntryCodeValidator<AttributionRolesVocabulary> attributionRolesValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Agent.Identifier)
            .NotEmpty();

        RuleFor(x => x.HadRole.Code)
            .SetValidator(attributionRolesValidator);
    }
}
