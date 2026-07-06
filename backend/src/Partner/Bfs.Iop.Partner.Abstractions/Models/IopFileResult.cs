namespace Bfs.Iop.Partner.Models;

/// <summary>
/// Wrapper for NSwag FileResponse type
/// </summary>
/// <param name="fileContents">File content</param>
/// <param name="contentType">Content type</param>
/// <param name="fileName">File name</param>
public sealed record IopFileResult(byte[] FileContents, string ContentType, string FileName)
{
    // ToDo: delete content type
}
