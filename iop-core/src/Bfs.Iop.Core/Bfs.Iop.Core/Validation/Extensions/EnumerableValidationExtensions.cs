using FluentValidation;

namespace Bfs.Iop.Core.Validation.Extensions;

internal static class EnumerableValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<U>> MustContainAtLeastOneItem<T, U>(
        this IRuleBuilder<T, IEnumerable<U>> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .NotNull()
            .Must(x => x.Any())
            .WithMessage(_ => "The collection must contain at least one item.");
    }
}
