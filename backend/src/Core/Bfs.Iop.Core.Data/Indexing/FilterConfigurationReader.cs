using System.Text.Json;
using Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.Core.Settings;

namespace Bfs.Iop.Core.Data.Indexing;

/// <inheritdoc cref="IFilterConfigurationReader"/>
/// <remarks>
/// Container name, file name and serializer settings are copied from
/// <c>FilterConfigurationFileStorageService</c> and must stay identical to it — the two read and
/// write the same blobs. A mismatch in any of the three does not fail: the file is simply never
/// found, and every annotation filter silently matches nothing.
/// </remarks>
internal sealed class FilterConfigurationReader : IFilterConfigurationReader
{
    private const string ContainerName = "filter-configuration";

    private static readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    private readonly IFileStorageService _fileStorageService;
    private readonly ApiSettings _apiSettings;

    public FilterConfigurationReader(IFileStorageService fileStorageService, ApiSettings apiSettings)
    {
        _fileStorageService = fileStorageService;
        _apiSettings = apiSettings;
    }

    public async Task<FilterConfigurationModel?> TryGetAsync(Guid conceptId, CancellationToken cancellationToken = default)
    {
        var container = $"{ContainerName}-{_apiSettings.EnvironmentName}";
        var fileName = $"{conceptId}.json";

        // Awaited, unlike the business-layer original, which blocks on .Result inside an async method.
        if (!await _fileStorageService.ExistsAsync(container, fileName, cancellationToken))
        {
            return null;
        }

        using var storedFile = await _fileStorageService.DownloadAsync(container, fileName, cancellationToken);

        return await JsonSerializer.DeserializeAsync<FilterConfigurationModel>(
            storedFile.Stream,
            _jsonSerializerOptions,
            cancellationToken);
    }
}
