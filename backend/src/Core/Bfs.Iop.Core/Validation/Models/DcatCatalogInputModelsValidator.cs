using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatCatalogInputModelsValidator : AbstractValidator<IEnumerable<DcatCatalogInputModel>>
{
    public DcatCatalogInputModelsValidator(IValidator<DcatCatalogInputModel> dcatCatalogInputModel) => 
        RuleForEach(x => x).SetValidator(dcatCatalogInputModel);
}
