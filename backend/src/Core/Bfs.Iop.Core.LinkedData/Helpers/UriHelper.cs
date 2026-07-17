namespace Bfs.Iop.Core.LinkedData.Helpers
{
    internal static class UriHelper
    {
        internal static string? GetLastElementFromUri(Uri uri)
        {
            if (!string.IsNullOrWhiteSpace(uri.Fragment))
            {
                return uri.Fragment;
            }
            var segments = uri.Segments;
            return segments.Length > 0 && segments[segments.Length - 1] != "/" ? segments[segments.Length - 1] : null;
        }

        internal static string GetLastUriElementOrUriComplete(Uri uri)
        {
            return GetLastElementFromUri(uri) ?? uri.AbsolutePath;
        }

        /// <summary>
        /// Returns a new URI where the last identifying element (fragment if present,
        /// otherwise the last non-empty path segment) is replaced by <paramref name="newIdentifier"/>.
        /// The rest of the URI (scheme, host, port, other path segments, query) is preserved.
        /// Returns <c>null</c> if <paramref name="uri"/> has no fragment and no non-empty last segment.
        /// </summary>
        internal static Uri? ReplaceLastElement(Uri uri, string newIdentifier)
        {
            ArgumentNullException.ThrowIfNull(uri);
            ArgumentException.ThrowIfNullOrEmpty(newIdentifier);

            var builder = new UriBuilder(uri);

            if (!string.IsNullOrEmpty(builder.Fragment))
            {
                // UriBuilder.Fragment must be set without the leading '#'
                builder.Fragment = newIdentifier;
                return builder.Uri;
            }

            var segments = uri.Segments;
            var lastSegmentIndex = -1;
            for (var i = segments.Length - 1; i >= 0; i--)
            {
                if (segments[i] != "/" && !string.IsNullOrEmpty(segments[i]))
                {
                    lastSegmentIndex = i;
                    break;
                }
            }

            if (lastSegmentIndex < 0)
            {
                return null;
            }

            // Segments already include trailing '/' for non-terminal segments; preserve trailing '/'
            // on the last segment if present.
            var lastSegment = segments[lastSegmentIndex];
            var hasTrailingSlash = lastSegment.EndsWith('/');

            var prefix = string.Concat(segments.Take(lastSegmentIndex));
            builder.Path = prefix + Uri.EscapeDataString(newIdentifier) + (hasTrailingSlash ? "/" : string.Empty);

            return builder.Uri;
        }
    }
}
