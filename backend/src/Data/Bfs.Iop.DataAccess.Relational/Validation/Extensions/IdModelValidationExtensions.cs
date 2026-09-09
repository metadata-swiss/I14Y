using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class IdModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<IdModel>> MustContainOnlyDistinctIds<T>(
        this IRuleBuilder<T, IEnumerable<IdModel>> rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            return rule
                .NotNull()
                .Must(ids => ids.DistinctBy(c => c.Id).Count() == ids.Count())
                .WithMessage(_ => "The collection cannot contain repeated ids.");
        }
}
