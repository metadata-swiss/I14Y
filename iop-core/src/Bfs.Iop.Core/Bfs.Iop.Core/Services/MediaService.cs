using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.FileStorage;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.Infrastructure.Security.Services;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.Core.Services;

internal sealed class MediaService : IMediaService
{
    private const string ContainerName = "media";

    private static IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDelete =>
        [
        BusinessRole.InteroperabilityService,
        BusinessRole.SwissDataSteward
        ];

    private readonly IFileStorageService _fileStorageService;
    private readonly IUserContextService _userContextService;
    private readonly string _openMediaBaseUrl;

    public MediaService(
        IFileStorageService fileStorageService, 
        IUserContextService userContextService,
        IOptions<I14YOptions> options)
    {
        _fileStorageService = fileStorageService
            ?? throw new ArgumentNullException(nameof(fileStorageService));

        _userContextService = userContextService
            ?? throw new ArgumentNullException(nameof(userContextService));

        var mediaBaseUrl = options?.Value.MediaBaseUrl;
        if (string.IsNullOrWhiteSpace(mediaBaseUrl))
            throw new ArgumentException("I14Y:MediaBaseUrl must be configured.", nameof(options));

        _openMediaBaseUrl = mediaBaseUrl.TrimEnd('/') + "/";
    }

    public async Task<ExportFile> GetFile(string url, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        var filename = GetFilename(url);

        if (!(await FileExists(filename, cancellationToken)))
        {
            throw new NotFoundException($"The file '{filename}' does not exist.");
        }

        var storedFile = await _fileStorageService.DownloadAsync(ContainerName, filename, cancellationToken);

        return new ExportFile(
            storedFile.Stream,
            storedFile.Metadata.Filename,
            storedFile.Metadata.ContentType ?? "application/octet-stream");
    }

    public async Task<IEnumerable<MediaInfoModel>> GetFileInfos(CancellationToken cancellationToken)
    {
        var metadata = await _fileStorageService.GetContainerContentAsync(ContainerName, cancellationToken);

        return metadata.Select(MapStoredFileMetadadaToMediaInfoModel);
    }

    public async Task<MediaInfoModel> UploadFile(IFormFile file, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(file, nameof(file));

        EnsureUserIsAllowed();

        var safeFilename = Path.GetFileName(file.FileName);

        if (await FileExists(safeFilename, cancellationToken))
        {
            throw new ConflictException($"A file with the name '{safeFilename}' already exists.");
        }

        using var stream = file.OpenReadStream();

        await _fileStorageService.UploadAsync(
            ContainerName,
            safeFilename,
            stream,
            file.ContentType,
            cancellationToken);

        var metadata = await _fileStorageService.GetFileMetadataAsync(ContainerName, safeFilename, cancellationToken);

        return MapStoredFileMetadadaToMediaInfoModel(metadata);
    }

    public async Task DeleteFile(string url, CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

        EnsureUserIsAllowed();

        var filename = GetFilename(url);

        if (!(await FileExists(filename, cancellationToken)))
        {
            throw new NotFoundException($"The file '{filename}' does not exist.");
        }

        await _fileStorageService.DeleteAsync(ContainerName, filename, cancellationToken);
    }

    private Task<bool> FileExists(string filename, CancellationToken cancellationToken) =>
        _fileStorageService.ExistsAsync(ContainerName, filename, cancellationToken);


    private void EnsureUserIsAllowed()
    {
        var businessRole = _userContextService.GetUserBusinessRole();

        if (!AllowedBusinessRolesToCreateUpdateDelete.Contains(businessRole))
        {
            throw _userContextService.IsUserTokenValid()
                ? new ForbiddenException("The user has not enough rights to perform the action.")
                : new UnauthorizedException("The user has no valid token.");
        }
    }

    private MediaInfoModel MapStoredFileMetadadaToMediaInfoModel(StoredFileMetadata metadata) => 
        new()
        {
            ContentLength = metadata.ContentLength,
            ContentType = metadata.ContentType,
            CreatedAt = metadata.CreatedAt,
            Filename = metadata.Filename,
            Url = $"{_openMediaBaseUrl}{Uri.EscapeDataString(metadata.Filename)}"
        };

    private string GetFilename(string url)
    {
        var decodedUrl = Uri.UnescapeDataString(url);

        var filename = decodedUrl.StartsWith(_openMediaBaseUrl, StringComparison.OrdinalIgnoreCase)
            ? decodedUrl[_openMediaBaseUrl.Length..]
            : decodedUrl;

        return Path.GetFileName(filename);
    }
}
