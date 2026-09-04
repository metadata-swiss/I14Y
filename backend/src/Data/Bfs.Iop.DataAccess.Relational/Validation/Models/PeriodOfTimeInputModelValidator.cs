using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class PeriodOfTimeInputModelValidator : AbstractValidator<PeriodOfTimeModel>
{
    public PeriodOfTimeInputModelValidator()
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(_ => _)
            .Must(x => x.Start.HasValue || x.End.HasValue)
            .WithMessage("At least one date should be provided.");

        RuleFor(_ => _)
            .Must(x => x.Start!.Value.CompareTo(x.End!.Value) <= 0)
            .When(x => x.Start.HasValue && x.End.HasValue)
            .WithMessage("The start date must be earlier than or equal to the end date.");
    }
}
