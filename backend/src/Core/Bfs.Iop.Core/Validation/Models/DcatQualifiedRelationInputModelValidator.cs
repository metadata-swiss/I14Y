using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatQualifiedRelationInputModelValidator : AbstractValidator<DcatQualifiedRelationInputModel>
{
    public DcatQualifiedRelationInputModelValidator(
        VocabularyEntryCodeValidator<RelationshipRolesVocabulary> relationshipRolesValidator,
        IValidator<ResourceModel> resourceInputModelValidator)
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.HadRole.Code)
            .SetValidator(relationshipRolesValidator);

        RuleFor(x => x.Relation)
            .SetValidator(resourceInputModelValidator);
    }
}
