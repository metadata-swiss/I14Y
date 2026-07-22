using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Common.Extensions;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.FileStorage.Services;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Helpers;
using Bfs.Iop.Core.Settings;
using Microsoft.AspNetCore.Http;
using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Services;

internal sealed class DatasetModelFileProcessService : IDatasetModelProcessService
{
    private const string Container = "dataset-structures";
    private readonly IDatasetsService _datasetsService;
    private readonly IFileStorageService _fileStorageService;
    private readonly ApiSettings _settings;

    public DatasetModelFileProcessService(
        IFileStorageService fileStorageService,
        ApiSettings apiSettings,
        IDatasetsService datasetsService)
    {
        _fileStorageService = fileStorageService;
        _settings = apiSettings;
        _datasetsService = datasetsService;
    }

    public Task DeleteGraph(Guid datasetId, CancellationToken cancellationToken)
    {
        EnsureUserIsAllowedToModifyDataset(datasetId);

        return _fileStorageService.DeleteAsync(GetContainer(), DatasetModelProcessHelper.GetFileName(LinkedDataFormat.Ttl, datasetId), cancellationToken);
    }

    public async Task<ExportFile> ExportGraph(LinkedDataFormat format, Guid datasetId, CancellationToken cancellationToken)
    {
        format.EnsureValueIsValid();

        EnsureUserIsAllowedToReadDataset(datasetId);

        string mimeType = DatasetModelProcessHelper.GetMimeType(format);
        string fileName = DatasetModelProcessHelper.GetFileName(format, datasetId);

        Graph graph = await LoadGraphAsync(datasetId, cancellationToken);

        Stream outputStream = new MemoryStream();
        DatasetModelProcessHelper.WriteGraphAccordingToFormat(graph, outputStream, format);

        return new ExportFile(outputStream, fileName, mimeType);
    }

    public async Task<IEnumerable<string>> GetAllDatasetIdsWithStructures(CancellationToken cancellationToken)
    {
        var infos = await _fileStorageService.GetContainerContentAsync(GetContainer(), cancellationToken);

        return infos.Select(x => x.Filename);
    }

    public async Task<SchemaGraph> GetDatasetModelGraphAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        EnsureUserIsAllowedToReadDataset(datasetId);
        var graph = await LoadGraphAsync(datasetId, cancellationToken);
        return DatasetModelProcessHelper.ConvertGraphToSchemaGraph(graph, $"https://www.i14y.admin.ch/resources/datasets/{datasetId}/");
    }

    public Task<bool> GraphExists(Guid datasetId, CancellationToken cancellationToken) =>
        _fileStorageService.ExistsAsync(GetContainer(), DatasetModelProcessHelper.GetFileName(LinkedDataFormat.Ttl, datasetId), cancellationToken);

    public Task<PagedResult<IopConceptStructureReferenceModel>> GetConceptStructureReferences(
        string conceptIdentifier,
        string conceptVersion,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        return Task.FromResult(new PagedResult<IopConceptStructureReferenceModel>()
        {
            Page = page,
            PageSize = pageSize,
            Results = [],
            TotalCount = 0,
        });
    }

    public Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetConceptStructureReferencesBatch(
        IEnumerable<IopConceptData> conceptsData,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>>(
            new Dictionary<Guid, IReadOnlyList<Guid>>());
    }

    public async Task UpdateClassesPosition(Guid datasetId, Dictionary<string, SchemaPoint> classesPositionInput, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(classesPositionInput, nameof(classesPositionInput));

        EnsureUserIsAllowedToModifyDataset(datasetId);

        var graph = await LoadGraphAsync(datasetId, cancellationToken);
        var query = ShaclSparqlQueryHelper.UpdateClassCoordinateQuery(classesPositionInput);
        ShaclSparqlQueryHelper.ExecuteUpdate(graph, query);

        await UpdateGraphFile(datasetId, graph, cancellationToken);
    }

    public Task UpdateClassOrProperty(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("this function is not support in this version");
    }

    public Task<Uri> CreateSchemaClass(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("this function is not support in this version");
    }

    public Task UpdateClassProperty(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken)
    {
        throw new NotSupportedException("this function is not support in this version");
    }

    public async Task UploadGraph(IFormFile importFile, Guid datasetId, CancellationToken cancellationToken)
    {
        EnsureUserIsAllowedToModifyDataset(datasetId);

        Graph graph = DatasetModelProcessHelper.ConvertFileToGraph(importFile, datasetId);

        using Stream outputStreamTtl = new MemoryStream();

        DatasetModelProcessHelper.WriteGraphAccordingToFormat(graph, outputStreamTtl, LinkedDataFormat.Ttl);

        await _fileStorageService.UploadAsync(
            GetContainer(),
            DatasetModelProcessHelper.GetFileName(LinkedDataFormat.Ttl, datasetId),
            outputStreamTtl,
            DatasetModelProcessHelper.GetMimeType(LinkedDataFormat.Ttl),
            cancellationToken);
    }

    private void EnsureUserIsAllowedToModifyDataset(Guid datasetId)
    {
        var allowAction = _datasetsService.GetUserAllowActionInfo(datasetId, cancellationToken: default).GetAwaiter().GetResult();

        if (!allowAction.Single(x => x.ActionType == AllowActionType.Edit).Value)
        {
            throw new ForbiddenException("No authorization to modify the resource.");
        }
    }

    private void EnsureUserIsAllowedToReadDataset(Guid datasetId)
    {
        // Easiest way
        _ = _datasetsService.GetDataset(datasetId, cancellationToken: default).GetAwaiter().GetResult();
    }

    private string GetContainer() => $"{Container}-{_settings.EnvironmentName}";

    private async Task<Graph> LoadGraphAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        var renderingFile = await _fileStorageService.DownloadAsync(
            GetContainer(),
            DatasetModelProcessHelper.GetFileName(LinkedDataFormat.Ttl, datasetId),
            cancellationToken);

        using var stream = renderingFile.Stream;

        return DatasetModelProcessHelper.LoadGraphAccordingToExtension(datasetId, stream, DatasetModelProcessHelper.TtlExtension);
    }

    private async Task UpdateGraphFile(Guid datasetId, Graph g, CancellationToken cancellationToken)
    {
        using Stream outputStream = new MemoryStream();
        DatasetModelProcessHelper.WriteGraphAccordingToFormat(g, outputStream, LinkedDataFormat.Ttl);

        if (await GraphExists(datasetId, cancellationToken))
        {
            await DeleteGraph(datasetId, cancellationToken);
        }

        await _fileStorageService.UploadAsync(
            GetContainer(),
            DatasetModelProcessHelper.GetFileName(LinkedDataFormat.Ttl, datasetId),
            outputStream,
            DatasetModelProcessHelper.GetMimeType(LinkedDataFormat.Ttl),
            cancellationToken);
    }
}