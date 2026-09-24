using Bfs.Iop.DataAccess.Abstractions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class ThemeInputModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<ThemeInputModel>> MustContainOnlyDistinctThemes<T>(
        this IRuleBuilder<T, IEnumerable<ThemeInputModel>> rule)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));

        return rule
            .NotNull()
            .Must(themes => themes.Where(t => t.Code is not null).DistinctBy(t => t.Code).Count() == themes.Count(t => t.Code is not null))
            .WithMessage(_ => "The collection cannot contain repeated theme codes.")
            .Must(themes => themes.Where(t => t.Uri is not null).DistinctBy(t => t.Uri).Count() == themes.Count(t => t.Uri is not null))
            .WithMessage(_ => "The collection cannot contain repeated theme URIs.");
    }
}
