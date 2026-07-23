using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Microsoft.AspNetCore.Http;

namespace Bfs.Iop.Core.LinkedData.Services;

public interface IDatasetModelProcessService
{
    Task DeleteGraph(Guid datasetId, CancellationToken cancellationToken);

    Task<ExportFile> ExportGraph(LinkedDataFormat format, Guid datasetId, CancellationToken cancellationToken);

    Task<IEnumerable<string>> GetAllDatasetIdsWithStructures(CancellationToken cancellationToken);

    Task<SchemaGraph> GetDatasetModelGraphAsync(Guid datasetId, CancellationToken cancellationToken);

    Task<bool> GraphExists(Guid datasetId, CancellationToken cancellationToken);

    Task<PagedResult<IopConceptStructureReferenceModel>> GetConceptStructureReferences(
        string conceptIdentifier, 
        string conceptVersion,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetConceptStructureReferencesBatch(
        IEnumerable<IopConceptData> conceptsData,
        CancellationToken cancellationToken = default);

    Task UpdateClassesPosition(Guid datasetId, Dictionary<string, SchemaPoint> classesPositionInput, CancellationToken cancellationToken);

    Task UpdateSchemaClass(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken);

    Task UpdateSchemaProperty(Guid datasetId, SchemaProperty propertyInput, Uri classUri, CancellationToken cancellationToken);

    Task<Uri> CreateSchemaClass(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken);

    Task<Uri> CreateSchemaProperty(Guid datasetId, SchemaProperty propertyInput, Uri classUri, CancellationToken cancellationToken);

    Task UploadGraph(IFormFile importFile, Guid datasetId, CancellationToken cancellationToken);
}