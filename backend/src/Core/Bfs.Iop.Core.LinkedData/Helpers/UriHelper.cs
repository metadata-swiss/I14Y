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
    }
}
