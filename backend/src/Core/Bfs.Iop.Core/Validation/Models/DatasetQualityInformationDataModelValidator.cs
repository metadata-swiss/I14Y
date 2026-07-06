using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DatasetQualityInformationDataModelValidator : AbstractValidator<DatasetQualityInformationDataModel>
{
    private readonly IEnumerable<DatasetQualityQuestion> _questions;

    public DatasetQualityInformationDataModelValidator(IopDbContext iopDbContext)
    {
        _questions = [.. iopDbContext.DatasetQualityQuestions];

        RuleFor(x => x.QualityInformations)
            .Must(x => x.Select(x => x.QuestionId).Distinct().Count() == x.Count())
            .WithMessage("Model cannot contain duplicated questions.");

        foreach (var question in _questions)
        {
            RuleFor(x => x.QualityInformations.SingleOrDefault(y => y.QuestionId == question.Id))
                .NotNull()
                .When(_ => question.Mandatory)
                .WithMessage($"The question '{question.Id}' is mandatory but it was not found.");
        }
    }
}
