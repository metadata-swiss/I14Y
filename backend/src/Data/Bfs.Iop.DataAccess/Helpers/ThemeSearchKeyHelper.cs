using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.DataAccess.Helpers;

// How a theme is named in the search index, the facets and the filter query parameter. A code alone
// would not do, since two taxonomies may share one, and the URI is too long for a URL. The separator
// is not allowed in an identifier, so only a code can contain one: the split takes the first.
public static class ThemeSearchKeyHelper
{
    public const char Separator = '!';

    public static string BuildKey(VocabularyEntryModel theme)
    {
        ArgumentNullException.ThrowIfNull(theme, nameof(theme));

        return string.IsNullOrWhiteSpace(theme.VocabularyIdentifier)
            ? theme.Code
            : $"{theme.VocabularyIdentifier}{Separator}{theme.Code}";
    }

    // Null when the value names no vocabulary, such as a bare code.
    public static (string VocabularyIdentifier, string Code)? SplitKey(string? key)
    {
        var separatorIndex = key?.IndexOf(Separator) ?? -1;

        return separatorIndex <= 0
            ? null
            : (key![..separatorIndex], key[(separatorIndex + 1)..]);
    }
}
