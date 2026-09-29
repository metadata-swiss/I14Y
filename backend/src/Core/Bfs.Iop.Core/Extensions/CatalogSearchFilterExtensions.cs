using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;

namespace Bfs.Iop.Core.Extensions;

internal static class CatalogSearchFilterExtensions
{
    /// <summary>
    /// Themes are indexed by "vocabulary!code", which is what the facet now hands back. A value that
    /// names no theme is read as a bare code and stands for every theme carrying it, so links shared
    /// before this change keep working — and filtering on a code that two taxonomies share returns
    /// both, which is what asking for a code rather than a theme should mean.
    /// </summary>
    public static CatalogSearchFilter WithResolvedThemes(this CatalogSearchFilter filter, IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(filter, nameof(filter));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        if (!filter.Themes.Any())
        {
            return filter;
        }

        var themes = vocabulariesService.GetThemes();
        var keys = themes.Select(ThemeSearchKey.For).ToHashSet(StringComparer.Ordinal);

        return filter with
        {
            Themes = filter.Themes
                .SelectMany(value => keys.Contains(value)
                    ? [value]
                    : themes.Where(theme => theme.Code == value).Select(ThemeSearchKey.For).DefaultIfEmpty(value))
                .Distinct(StringComparer.Ordinal)
                .ToList()
        };
    }
}
