using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Extensions;

internal static class ThemeInputModelValidationExtensions
{
    public static IRuleBuilderOptions<T, IEnumerable<ThemeInputModel>> MustContainOnlyDistinctThemes<T>(
        this IRuleBuilder<T, IEnumerable<ThemeInputModel>> rule,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(rule, nameof(rule));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        return rule
            .NotNull()
            .Must(themes => themes
                .DistinctBy(theme => CanonicalKeyOf(theme, vocabulariesService), StringComparer.Ordinal)
                .Count() == themes.Count())
            .WithMessage(_ => "The collection cannot name the same theme twice.");
    }

    // The same theme can be named by code or by URI, so uniqueness is decided on what it resolves to
    // rather than on the fields given. An input resolving to nothing keeps what was written, and
    // ThemeInputModelValidator is what reports it.
    private static string CanonicalKeyOf(ThemeInputModel theme, IVocabulariesService vocabulariesService)
    {
        if (!string.IsNullOrWhiteSpace(theme.Uri))
        {
            return theme.Uri;
        }

        var uris = vocabulariesService.GetThemes()
            .Where(x => x.Code == theme.Code)
            .Select(x => x.Uri!)
            .Distinct(StringComparer.Ordinal)
            .ToList();

        return uris.Count == 1 ? uris[0] : theme.Code ?? string.Empty;
    }
}
