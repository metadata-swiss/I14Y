using Bfs.Iop.Core.ApiClient;
using Microsoft.Net.Http.Headers;

namespace Bfs.Iop.Partner.Business.Extensions;

public static class FileResponseExtensions
{
    public static string GetFileNameFromHeader(this FileResponse fileResponse, string defaultFileName = "")
    {
        return fileResponse
            .Headers[HeaderNames.ContentDisposition]
                .First()
                .Split(';')
                .Select(x => x.Trim())
                .ElementAt(1).Replace("filename=", "")
                ?? defaultFileName;
    }

    public static string GetContentTypeFromHeader(this FileResponse fileResponse, string defaultContentType = "application/json")
    {
        return fileResponse.Headers[HeaderNames.ContentType].FirstOrDefault() ?? defaultContentType;
    }
}
