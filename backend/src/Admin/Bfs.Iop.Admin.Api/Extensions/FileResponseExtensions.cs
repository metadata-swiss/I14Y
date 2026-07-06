using Bfs.Iop.Core.ApiClient;
using Microsoft.Net.Http.Headers;
using System.Linq;

namespace Bfs.Iop.Admin.Api.Extensions;

internal static class FileResponseExtensions
{
    public static string GetFileNameFromHeader(this FileResponse fileResponse, string defaultFileName = "") =>
        fileResponse
            .Headers[HeaderNames.ContentDisposition]
                .First()
                .Split(';')
                .Select(x => x.Trim())
                .ElementAt(1).Replace("filename=", "")
                ?? defaultFileName;

    public static string GetContentTypeFromHeader(this FileResponse fileResponse, string defaultContentType = "application/json") =>
        fileResponse.Headers[HeaderNames.ContentType].FirstOrDefault() ?? defaultContentType;
}
