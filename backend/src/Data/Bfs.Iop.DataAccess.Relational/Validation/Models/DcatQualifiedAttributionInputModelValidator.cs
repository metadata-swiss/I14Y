using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class DcatQualifiedAttributionInputModelValidator : AbstractValidator<DcatQualifiedAttributionInputModel>
{
    public DcatQualifiedAttributionInputModelValidator(
        VocabularyEntryCodeValidator<AttributionRolesVocabulary> attributionRolesValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Agent)
            .MustBeValidAgentInput();

        RuleFor(x => x.HadRole.Code)
            .SetValidator(attributionRolesValidator);
    }
}
