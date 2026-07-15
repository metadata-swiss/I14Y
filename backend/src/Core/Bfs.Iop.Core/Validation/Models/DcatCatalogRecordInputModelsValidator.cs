using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatCatalogRecordInputModelsValidator : AbstractValidator<IEnumerable<DcatCatalogRecordInputModel>>
{
    public DcatCatalogRecordInputModelsValidator(IValidator<DcatCatalogRecordInputModel> dcatCatalogRecordInputModel) => 
        RuleForEach(x => x).SetValidator(dcatCatalogRecordInputModel);
}
