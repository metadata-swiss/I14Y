using Bfs.Iop.Core.Abstractions.Models;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Extensions;

internal static class IdentifierInputModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<IdentifierInputModel>> MustContainOnlyDistinctIdentifiers<T>(
        this IRuleBuilder<T, IEnumerable<IdentifierInputModel>> rule)
        {
            ArgumentNullException.ThrowIfNull(rule, nameof(rule));

            return rule
                .NotNull()
                .Must(identifiers => identifiers.DistinctBy(c => c.Identifier).Count() == identifiers.Count())
                .WithMessage(_ => "The collection cannot contain repeated identifiers.");
        }
}
