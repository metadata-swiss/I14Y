using Bfs.Iop.Core.Abstractions.Models;
using Microsoft.AspNetCore.Http;

namespace Bfs.Iop.Core.Services.Contracts;

internal interface IMediaService
{
    Task<ExportFile> GetFile(string url, CancellationToken cancellationToken);

    Task<IEnumerable<MediaInfoModel>> GetFileInfos(CancellationToken cancellationToken);

    Task<MediaInfoModel> UploadFile(IFormFile file, CancellationToken cancellationToken);

    Task DeleteFile(string url, CancellationToken cancellationToken);
}
