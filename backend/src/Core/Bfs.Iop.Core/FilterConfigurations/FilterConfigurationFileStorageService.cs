using Bfs.Iop.Common.Settings;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.Extensions;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.DataAccess.Abstractions.Exceptions;
using Bfs.Iop.Infrastructure.Security;
using Bfs.Iop.Infrastructure.Security.Services;
using System.Text.Json;

namespace Bfs.Iop.Core.FilterConfigurations;

internal sealed class FilterConfigurationFileStorageService
{
    private const string ContainerName = "filter-configuration";

    private readonly IFileStorageService _fileStorageService;
    private readonly ApiSettings _apiSettings;

    private readonly IUserContextService _userContextService;

    private static IEnumerable<BusinessRole> AllowedBusinessRolesToCreateUpdateDeleteObject =>
        [
        BusinessRole.InteroperabilityService,
        ];

    private readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FilterConfigurationFileStorageService(
        IFileStorageService fileStorageService,
        ApiSettings apiSettings,
        IUserContextService userContextService)
    {
        _fileStorageService = fileStorageService;
        _apiSettings = apiSettings;
        _userContextService = userContextService;
    }

    public async Task UploadAsync(Guid id, Stream content, CancellationToken cancellationToken = default)
    {
        _userContextService.EnsureUserIsAllowed(AllowedBusinessRolesToCreateUpdateDeleteObject);

        await ValidateStreamAsync(id, content, cancellationToken);

        await _fileStorageService.UploadAsync(GetContainerName(), GetFileName(id), content, "application/json", cancellationToken);
    }

    public async Task<FilterConfigurationModel> DownloadAsync(Guid id, CancellationToken cancellationToken = default)
    {
        if (!_fileStorageService.ExistsAsync(GetContainerName(), GetFileName(id), cancellationToken).Result)
        {
            throw new NotFoundException("The requested filter configuration was not found");
        }

        using var storedFile = await _fileStorageService.DownloadAsync(GetContainerName(), GetFileName(id), cancellationToken);

        FilterConfigurationModel result = await JsonSerializer.DeserializeAsync<FilterConfigurationModel>(storedFile.Stream,
            _jsonSerializerOptions, cancellationToken)
            ?? throw new InvalidOperationException("GetFilterConfigurationCommandHandler can not deserialize saved FilterConfigurationModel!");

        return result;
    }

    public Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        _userContextService.EnsureUserIsAllowed(AllowedBusinessRolesToCreateUpdateDeleteObject);

        return _fileStorageService.DeleteAsync(GetContainerName(), GetFileName(id), cancellationToken);
    }

    private async Task ValidateStreamAsync(Guid id, Stream inputStream, CancellationToken cancellationToken = default)
    {
        using var copy = new MemoryStream();
        await inputStream.CopyToAsync(copy, cancellationToken);
        copy.Position = 0;

        _ = await JsonSerializer.DeserializeAsync<FilterConfigurationModel>(copy, _jsonSerializerOptions, cancellationToken)
            ?? throw new InvalidOperationException($"Invalid filter configuration for object {id}.");
    }

    private string GetContainerName() => $"{ContainerName}-{_apiSettings.EnvironmentName}";

    private static string GetFileName(Guid id) => $"{id}.json";
}