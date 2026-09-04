using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class CodeInputModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<CodeInputModel>> MustContainOnlyDistinctCodes<T>(
        this IRuleBuilder<T, IEnumerable<CodeInputModel>> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .NotNull()
            .Must(codes => codes.DistinctBy(c => c.Code).Count() == codes.Count())
            .WithMessage(_ => "The collection cannot contain repeated codes.");
    }
}
