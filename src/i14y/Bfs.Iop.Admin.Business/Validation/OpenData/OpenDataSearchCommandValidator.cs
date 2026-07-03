using Bfs.Iop.Admin.Commands.OpenData.Search;
using FluentValidation;

namespace Bfs.Iop.Admin.Business.Validation.OpenData;

public sealed class OpenDataSearchCommandValidator : AbstractValidator<OpenDataSearchCommand>
{
    public OpenDataSearchCommandValidator()
    {
        RuleFor(x => x.Culture).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0).WithMessage($"The page must be greater than 0");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage($"The page size must be greater than 0");
    }
}