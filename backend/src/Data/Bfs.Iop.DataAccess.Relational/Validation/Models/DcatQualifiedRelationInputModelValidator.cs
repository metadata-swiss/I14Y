using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

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
