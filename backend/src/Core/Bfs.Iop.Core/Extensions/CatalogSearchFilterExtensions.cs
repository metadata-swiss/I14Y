using Bfs.Iop.Core.Abstractions.Models.Search.Filters;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;

namespace Bfs.Iop.Core.Extensions;

internal static class CatalogSearchFilterExtensions
{
    // A value that is not a theme key is taken for a bare code, so links shared before this change
    // keep working and a code two taxonomies share filters on both.
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
