using Bfs.Iop.Admin.Commands.Geocat.Search;
using FluentValidation;

namespace Bfs.Iop.Admin.Business.Commands.Geocat;

public sealed class GeocatSearchCommandValidator : AbstractValidator<GeocatSearchCommand>
{
    public GeocatSearchCommandValidator()
    {
        RuleFor(x => x.Culture).NotEmpty();
        RuleFor(x => x.Page).GreaterThan(0).WithMessage($"The page must be greater than 0");
        RuleFor(x => x.PageSize).GreaterThan(0).WithMessage($"The page size must be greater than 0");
    }
}