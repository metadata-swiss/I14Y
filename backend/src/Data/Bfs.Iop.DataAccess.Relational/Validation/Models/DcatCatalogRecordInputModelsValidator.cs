using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class DcatCatalogRecordInputModelsValidator : AbstractValidator<IEnumerable<DcatCatalogRecordInputModel>>
{
    public DcatCatalogRecordInputModelsValidator(IValidator<DcatCatalogRecordInputModel> dcatCatalogRecordInputModel)
    {
        RuleFor(x => x)
            .MustContainAtLeastOneItem();

        RuleFor(x => x)
            .Must(x => x.Select(m => m.PrimaryTopic.ResourceId).Distinct().Count() == x.Count())
            .WithMessage("Only one catalog record per resource is allowed.");

        RuleForEach(x => x).SetValidator(dcatCatalogRecordInputModel);
    }
}
