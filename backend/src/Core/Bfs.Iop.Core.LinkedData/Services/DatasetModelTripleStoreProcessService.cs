using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.DataObjects;
using Bfs.Iop.Core.LinkedData.Factories;
using Bfs.Iop.Core.LinkedData.Helpers;
using Bfs.Iop.Core.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Query;
using VDS.RDF.Storage;

namespace Bfs.Iop.Core.LinkedData.Services;

internal sealed class DatasetModelTripleStoreProcessService : IDatasetModelProcessService, IDisposable
{
    private readonly IDatasetsService _datasetsService;
    private readonly FusekiConnector _tripleStoreConnection;
    private readonly SparqlQueryClient _queryClient;
    private readonly string _baseIriUrl;

    public DatasetModelTripleStoreProcessService(
        FusekiConnectionFactory tripleStoreConnectionFactory,
        IDatasetsService datasetsService,
        IOptions<I14YOptions> i14yOptions)
    {
        _tripleStoreConnection = tripleStoreConnectionFactory.CreateFusekiConnector();

        _queryClient = tripleStoreConnectionFactory.CreateQueryClient();
        _datasetsService = datasetsService;
        _baseIriUrl = i14yOptions.Value.IriBaseUrl.TrimEnd('/');
    }

    public void Dispose()
    {
        _tripleStoreConnection.Dispose();
    }

    public async Task DeleteGraph(Guid datasetId, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        var recursiveTraversal = await RequiresRecursiveStructureTraversal(datasetId, cancellationToken);

        var deleteQuery = ShaclSparqlQueryHelper.DeleteStructureQuery(
            datasetId,
            recursiveTraversal);

        await ExecuteUpdateAsync(deleteQuery, cancellationToken);
    }

    public async Task<ExportFile> ExportGraph(LinkedDataFormat format, Guid datasetId, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToReadDataset(datasetId, cancellationToken);

        if (!await GraphExists(datasetId, cancellationToken))
        {
            throw new NotFoundException($"No structure found for dataset '{datasetId}'.");
        }

        string mimeType = DatasetModelProcessHelper.GetMimeType(format);
        string fileName = DatasetModelProcessHelper.GetFileName(format, datasetId);

        var recursiveTraversal = await RequiresRecursiveStructureTraversal(datasetId, cancellationToken);

        var query = ShaclSparqlQueryHelper.ConstructStructureQuery(datasetId, recursiveTraversal);

        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is not Graph graph)
        {
            throw new InvalidOperationException("Expected a graph result when exporting a structure.");
        }

        ShaclSparqlQueryHelper.CleanStructureBeforeExport(graph, datasetId);

        Stream outputStream = new MemoryStream();
        DatasetModelProcessHelper.WriteGraphAccordingToFormat(graph, outputStream, format);

        return new ExportFile(outputStream, fileName, mimeType);
    }

    public async Task<IEnumerable<string>> GetAllDatasetIdsWithStructures(CancellationToken cancellationToken)
    {
        var query = ShaclSparqlQueryHelper.ListStructuresQuery();

        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is not SparqlResultSet resultSet)
        {
            throw new InvalidOperationException("Expected a SPARQL result set when listing structures.");
        }

        var datasetIds = new List<string>();

        foreach (var result in resultSet)
        {
            if (!result.HasValue("datasetId") || result["datasetId"] is not ILiteralNode datasetIdNode)
            {
                continue;
            }

            var datasetId = datasetIdNode.Value;

            if (string.IsNullOrWhiteSpace(datasetId))
            {
                continue;
            }

            datasetIds.Add(datasetId);
        }

        return datasetIds;
    }

    public async Task<SchemaGraph> GetDatasetModelGraphAsync(Guid datasetId, CancellationToken cancellationToken)
    {
        // EnsureUserIsAllowedToReadDataset not needed because we make GetDataset
        var dataset = await _datasetsService.GetDataset(datasetId, cancellationToken);
        var datasetIdentifier = dataset.Identifiers.First();

        var recursiveTraversal = await RequiresRecursiveStructureTraversal(datasetId, cancellationToken);

        var query = ShaclSparqlQueryHelper.ConstructStructureQuery(datasetId, recursiveTraversal);

        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is not Graph graph)
        {
            throw new InvalidOperationException("Expected a graph result when loading a dataset structure.");
        }

        return DatasetModelProcessHelper.ConvertGraphToSchemaGraph(graph, BuildDatasetIri(datasetIdentifier));
    }

    public async Task<bool> GraphExists(Guid datasetId, CancellationToken cancellationToken)
    {
        try
        {
            var query = ShaclSparqlQueryHelper.StructureExistsQuery(datasetId);

            var queryResult = await ExecuteQueryAsync(query, cancellationToken);

            if (queryResult is not SparqlResultSet resultSet)
            {
                throw new InvalidOperationException("Expected a SPARQL result set for structure existence query.");
            }

            return resultSet.Result;
        }
        catch (NotFoundException)
        {
            return false;
        }
    }

    public async Task<PagedResult<IopConceptStructureReferenceModel>> GetConceptStructureReferences(
        string conceptIdentifier,
        string conceptVersion,
        int page,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(conceptIdentifier, nameof(conceptIdentifier));
        ArgumentException.ThrowIfNullOrWhiteSpace(conceptVersion, nameof(conceptVersion));

        var conceptReferences = await GetAllConceptStructureReferences(conceptIdentifier, conceptVersion, cancellationToken);

        var pagedResult = new PagedResult<IopConceptStructureReferenceModel>
        {
            Results = conceptReferences
                .Skip((page - 1) * pageSize)
                .Take(pageSize),
            Page = page,
            PageSize = pageSize is int.MaxValue ? conceptReferences.Count() : pageSize,
            TotalCount = conceptReferences.Count()
        };

        return pagedResult;
    }

    public async Task UpdateClassesPosition(Guid datasetId, Dictionary<string, SchemaPoint> classesPositionInput, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        ArgumentNullException.ThrowIfNull(classesPositionInput, nameof(classesPositionInput));

        var query = ShaclSparqlQueryHelper.UpdateClassCoordinateQuery(
            classesPositionInput,
            datasetId: datasetId);

        await ExecuteUpdateAsync(query, cancellationToken);
    }

    public async Task UpdateSchemaClass(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        ArgumentNullException.ThrowIfNull(schemaClassInput, nameof(schemaClassInput));

        if (string.IsNullOrWhiteSpace(schemaClassInput.UriComplete.AbsoluteUri))
        {
            throw new ArgumentException("The input is not valid. Uri of class cannot be empty");
        }

        await UpdateClass(datasetId, schemaClassInput, schemaClassInput.UriComplete, cancellationToken);
        if (!string.IsNullOrWhiteSpace(schemaClassInput.Identifier))
        {
            await UpdateClassUribyIdentifier(datasetId, schemaClassInput, cancellationToken);
        }
    }

    public async Task UpdateSchemaProperty(Guid datasetId, SchemaProperty schemaPropertyInput, Uri classUri, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        ArgumentNullException.ThrowIfNull(schemaPropertyInput, nameof(schemaPropertyInput));
        ArgumentNullException.ThrowIfNull(classUri, nameof(classUri));

        if (string.IsNullOrWhiteSpace(schemaPropertyInput.Path.AbsoluteUri))
        {
            throw new ArgumentException("The input is not valid. Path of property cannot be empty");
        }

        await UpdateProperty(datasetId, schemaPropertyInput, classUri, cancellationToken);
        if (!string.IsNullOrWhiteSpace(schemaPropertyInput.Identifier))
        {
            await UpdatePropertyUribyIdentifier(datasetId, classUri, schemaPropertyInput, cancellationToken);
        }
    }

    public async Task UploadGraph(IFormFile importFile, Guid datasetId, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        var dataset = await _datasetsService.GetDataset(datasetId, cancellationToken);
        var datasetIdentifier = dataset.Identifiers.First();

        Graph graph = DatasetModelProcessHelper.ConvertFileToGraph(importFile, datasetId);

        graph = await DatasetStructureImportTransformer.TransformImportedGraph(
            graph,
            datasetId,
            datasetIdentifier,
            _baseIriUrl,
            cancellationToken);

        var recursiveTraversal = await RequiresRecursiveStructureTraversal(datasetId, cancellationToken);

        var deleteQuery = ShaclSparqlQueryHelper.DeleteStructureQuery(
            datasetId,
            recursiveTraversal);

        await ExecuteUpdateAsync(deleteQuery, cancellationToken);

        await UpdateGraphAsync(
            graph.Triples,
            Enumerable.Empty<Triple>(),
            cancellationToken);
    }

    private async Task UpdateProperty(Guid datasetId, SchemaProperty schemaPropertyInput, Uri classUri, CancellationToken cancellationToken)
    {
        var query = ShaclSparqlQueryHelper.UpdatePropertyQuery(
            schemaPropertyInput,
            datasetId,
            classUri);

        await ExecuteUpdateAsync(query, cancellationToken);
    }

    private async Task UpdateClass(Guid datasetId, SchemaClass schemaClassInput, Uri classUri, CancellationToken cancellationToken)
    {
        var query = ShaclSparqlQueryHelper.UpdateClassQuery(
             schemaClassInput,
             datasetId);

        await ExecuteUpdateAsync(query, cancellationToken);
    }


    private async Task UpdatePropertyUribyIdentifier(Guid datasetId, Uri classUri, SchemaProperty propertyInput, CancellationToken cancellationToken)
    {
        var oldPropertyUriIdentifier = UriHelper.GetLastElementFromUri(propertyInput.Path);
        if (oldPropertyUriIdentifier != null && propertyInput.Identifier != null && oldPropertyUriIdentifier != propertyInput.Identifier)
        {
            var newPath = UriHelper.ReplaceLastElement(propertyInput.Path, propertyInput.Identifier);
            if (newPath == null)
            {
                return;
            }

            var query = ShaclSparqlQueryHelper.UpdatePropertyUriQuery(
                datasetId,
                propertyInput.Path,
                newPath,
                classUri);
            await ExecuteUpdateAsync(query, cancellationToken);
        }
    }

    private async Task<int?> GetPropertyCountFromClass(SchemaClass classInput, CancellationToken cancellationToken)
    {
        var query = ShaclSparqlQueryHelper.GetPropertyCountFromClassQuery(classInput.UriComplete);
        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is SparqlResultSet resultSet && resultSet.Count > 0)
        {
            resultSet[0].TryGetValue(ShaclSparqlQueryHelper.PropertyCountColumn, out INode? nodeCountProperty);
            return int.TryParse((nodeCountProperty as LiteralNode)?.Value, out var count) ? count : null;
        }

        return null;
    }


    private async Task UpdateClassUribyIdentifier(Guid datasetId, SchemaClass classInput, CancellationToken cancellationToken)
    {
        var oldClassUriIdentifier = UriHelper.GetLastElementFromUri(classInput.UriComplete);
        if (oldClassUriIdentifier == null)
        {
            throw new ArgumentException("The input is not valid. Can't find old Class");
        }

        if (oldClassUriIdentifier == classInput.Identifier)
        {
            return;
        }

        var propertyCount = await GetPropertyCountFromClass(classInput, cancellationToken);
        if (propertyCount is not 0)
        {
            return;
        }

        var newClassUri = UriHelper.ReplaceLastElement(classInput.UriComplete, classInput.Identifier);
        if (newClassUri == null)
        {
            return;
        }

        var query = ShaclSparqlQueryHelper.UpdateClassUriQuery(
            datasetId,
            classInput.UriComplete,
            newClassUri);

        await ExecuteUpdateAsync(query, cancellationToken);
    }

    public async Task<Uri> CreateSchemaClass(Guid datasetId, SchemaClass schemaClassInput, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        ArgumentNullException.ThrowIfNull(schemaClassInput, nameof(schemaClassInput));

        if (schemaClassInput.UriComplete == null || string.IsNullOrWhiteSpace(schemaClassInput.UriComplete.AbsoluteUri))
        {
            throw new ArgumentException("The input is not valid. Uri of class cannot be empty");
        }

        var newClassUri = BuildNewClassUri(schemaClassInput);
        if (newClassUri == null)
        {
            throw new ArgumentException("The input is not valid. Identifier is required when UriComplete ends with '/'.");
        }

        var dataset = await _datasetsService.GetDataset(datasetId, cancellationToken);
        var datasetIdentifier = dataset.Identifiers.First();
        var structureRootUri = new Uri(ShaclSparqlQueryHelper.GetStructureRootUri(datasetIdentifier, _baseIriUrl));

        var query = ShaclSparqlQueryHelper.CreateSchemaClassQuery(datasetId, schemaClassInput, newClassUri, structureRootUri);
        await ExecuteUpdateAsync(query, cancellationToken);

        return newClassUri;
    }

    public async Task<Uri> CreateSchemaProperty(Guid datasetId, SchemaProperty propertyInput, Uri classUri, CancellationToken cancellationToken)
    {
        await EnsureUserIsAllowedToModifyDataset(datasetId, cancellationToken);

        ArgumentNullException.ThrowIfNull(propertyInput, nameof(propertyInput));
        ArgumentNullException.ThrowIfNull(classUri, nameof(classUri));

        // Property shape URI follows the same convention used elsewhere in the codebase
        // If neither UriComplete nor Path is provided, build one from classUri + '/' + Identifier.
        var propertyUri =  propertyInput.Path ?? propertyInput.UriComplete ?? BuildPropertyUriFromClass(classUri, propertyInput.Identifier);

        // 1) Insert the base PropertyShape (sh:property + sh:path).
        var createQuery = ShaclSparqlQueryHelper.CreateSchemaPropertyQuery(classUri, propertyUri);
        await ExecuteUpdateAsync(createQuery, cancellationToken);

        // 2) Fill in all attributes (label, description, cardinalities, pattern, datatype,
        // conformsTo, unit, allowedValues, order, ...) via the existing update query.
        var updateQuery = ShaclSparqlQueryHelper.UpdatePropertyQuery(propertyInput, datasetId, classUri);
        await ExecuteUpdateAsync(updateQuery, cancellationToken);

        return propertyUri;
    }

    private static Uri BuildPropertyUriFromClass(Uri classUri, string? identifier)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            throw new ArgumentException("The input is not valid. Property must have UriComplete, Path, or Identifier.");
        }

        var baseUri = classUri.AbsoluteUri.EndsWith('/') ? classUri.AbsoluteUri : classUri.AbsoluteUri + "/";
        return new Uri(baseUri + Uri.EscapeDataString(identifier));
    }

    /// <summary>
    /// Builds the URI for a newly created class:
    /// - if <c>UriComplete</c> ends with '/', it is treated as a prefix and combined with <c>Identifier</c>;
    /// - otherwise, <c>UriComplete</c> is used directly as the class URI.
    /// Returns <c>null</c> if the required inputs are missing.
    /// </summary>
    private static Uri? BuildNewClassUri(SchemaClass classInput)
    {
        var uriComplete = classInput.UriComplete.AbsoluteUri;

        if (uriComplete.EndsWith('/'))
        {
            if (string.IsNullOrWhiteSpace(classInput.Identifier))
            {
                return null;
            }

            return new Uri(uriComplete + Uri.EscapeDataString(classInput.Identifier));
        }

        return classInput.UriComplete;
    }

    private async Task EnsureUserIsAllowedToReadDataset(Guid datasetId, CancellationToken cancellationToken) =>
        //Raises exception if user is not allowed to read
        await _datasetsService.GetDataset(datasetId, cancellationToken);

    private async Task EnsureUserIsAllowedToModifyDataset(Guid datasetId, CancellationToken cancellationToken)
    {
        var allowAction = await _datasetsService.GetUserAllowActionInfo(datasetId, cancellationToken);

        if (!allowAction.Single(x => x.ActionType == AllowActionType.Edit).Value)
        {
            throw new ForbiddenException("No authorization to modify the resource.");
        }
    }

    private async Task<bool> RequiresRecursiveStructureTraversal(
        Guid datasetId,
        CancellationToken cancellationToken)
    {
        var askQuery = ShaclSparqlQueryHelper.RequiresRecursiveStructureTraversalQuery(datasetId);
        var queryResult = await ExecuteQueryAsync(askQuery, cancellationToken);

        if (queryResult is not SparqlResultSet resultSet)
        {
            throw new InvalidOperationException("Expected a SPARQL result set for protected traversal detection query.");
        }

        return resultSet.Result;
    }

    public async Task<IReadOnlyDictionary<Guid, IReadOnlyList<Guid>>> GetConceptStructureReferencesBatch(
        IEnumerable<IopConceptData> conceptsData,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(conceptsData, nameof(conceptsData));

        var iriToId = new Dictionary<string, Guid>();
        foreach (var conceptData in conceptsData)
        {
            iriToId[BuildConceptIri(conceptData.Identifier, conceptData.Version)] = conceptData.Id;
        }

        if (iriToId.Count == 0)
        {
            return ReadOnlyEmptyReferences;
        }

        // One datasetId per (concept, dataset, property) reference row. The caller filters these
        // datasetIds by the user's read-authorization and counts the survivors.
        var references = iriToId.Values.ToDictionary(id => id, _ => (IReadOnlyList<Guid>)new List<Guid>());

        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery(iriToId.Keys);
        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is not SparqlResultSet resultSet)
        {
            return references.AsReadOnly();
        }

        foreach (var result in resultSet)
        {
            if (result[ShaclSparqlQueryHelper.ConceptUriColumn] is not IUriNode conceptUriNode ||
                !iriToId.TryGetValue(conceptUriNode.Uri.AbsoluteUri, out var conceptId))
            {
                continue;
            }

            if (result[ShaclSparqlQueryHelper.DatasetIdColumn] is ILiteralNode datasetIdNode &&
                Guid.TryParse(datasetIdNode.Value, out var datasetId))
            {
                ((List<Guid>)references[conceptId]).Add(datasetId);
            }
        }

        return references.AsReadOnly();
    }

    private static readonly IReadOnlyDictionary<Guid, IReadOnlyList<Guid>> ReadOnlyEmptyReferences =
        new Dictionary<Guid, IReadOnlyList<Guid>>().AsReadOnly();

    private async Task<IEnumerable<IopConceptStructureReferenceModel>> GetAllConceptStructureReferences(string identifier, string version, CancellationToken cancellationToken)
    {
        var conceptUri = new Uri(BuildConceptIri(identifier, version));

        var query = ShaclSparqlQueryHelper.GetConceptConformsToReuseQuery(conceptUri);

        var queryResult = await ExecuteQueryAsync(query, cancellationToken);

        if (queryResult is not SparqlResultSet resultSet)
        {
            throw new InvalidOperationException("Expected a SPARQL result set when searching concept usage.");
        }

        var conceptReferences = new List<IopConceptStructureReferenceModel>();
        var datasetUriById = new Dictionary<Guid, string>();
        var inaccessibleDatasetIds = new HashSet<Guid>();

        foreach (var result in resultSet)
        {
            if (!result.HasValue("datasetId") ||
                result["datasetId"] is not ILiteralNode datasetIdNode ||
                !Guid.TryParse(datasetIdNode.Value, out var datasetId) ||
                !result.HasValue("propertyUri") ||
                result["propertyUri"] is not IUriNode propertyUriNode)
            {
                continue;
            }

            if (inaccessibleDatasetIds.Contains(datasetId))
            {
                continue;
            }

            if (!datasetUriById.TryGetValue(datasetId, out var datasetUri))
            {
                try
                {
                    var dataset = await _datasetsService.GetDataset(datasetId, cancellationToken);
                    var datasetIdentifier = dataset.Identifiers.First();

                    datasetUri = BuildDatasetIri(datasetIdentifier);
                    datasetUriById.Add(datasetId, datasetUri);
                }
                catch (Exception ex) when (ex is NotFoundException or ForbiddenException or UnauthorizedException)
                {
                    inaccessibleDatasetIds.Add(datasetId);
                    continue;
                }
            }

            conceptReferences.Add(new IopConceptStructureReferenceModel(datasetUri, propertyUriNode.Uri.AbsoluteUri));

        }
        return conceptReferences;
    }

    private string BuildDatasetIri(string datasetIdentifier) =>
        $"{_baseIriUrl}/dataset/{datasetIdentifier}";

    private string BuildConceptIri(string conceptIdentifier, string conceptVersion) =>
        $"{_baseIriUrl}/concept/{conceptIdentifier}/version/{conceptVersion}";


    private async Task<object> ExecuteQueryAsync(string query, CancellationToken cancellationToken)
    {
        var parser = new SparqlQueryParser();
        var parsedQuery = parser.ParseFromString(query);

        return parsedQuery.QueryType switch
        {
            SparqlQueryType.Select or SparqlQueryType.SelectDistinct or SparqlQueryType.Ask => await _queryClient.QueryWithResultSetAsync(query, cancellationToken),
            SparqlQueryType.Construct or SparqlQueryType.Describe => await _queryClient.QueryWithResultGraphAsync(query, cancellationToken),
            _ => throw new NotSupportedException($"Unsupported SPARQL query type: {parsedQuery.QueryType}"),
        };
    }

    private Task ExecuteUpdateAsync(string updateQuery, CancellationToken cancellationToken)
    {
        // NOTE: Using sync FusekiConnector methods on purpose
        // Async versions leak HTTP responses (observed: hangs after ~5 calls)
        // Once async methods are fixed in a future dotNetRDF version, this wrapper can be replaced
        return Task.Run(() => _tripleStoreConnection.Update(updateQuery), cancellationToken);
    }

    private Task UpdateGraphAsync(
        IEnumerable<Triple> triplesToAdd,
        IEnumerable<Triple> triplesToRemove,
        CancellationToken cancellationToken)
    {
        // Materialize triples before offloading to avoid enumerating graph-backed collections on another thread
        var triplesToAddList = triplesToAdd.ToList();
        var triplesToRemoveList = triplesToRemove.ToList();

        // NOTE: Using sync FusekiConnector methods on purpose
        // Async versions leak HTTP responses (observed: hangs after ~5 calls)
        // Once async methods are fixed in a future dotNetRDF version, this wrapper can be replaced
        return Task.Run(
            () => _tripleStoreConnection.UpdateGraph(
                (string?)null,
                triplesToAddList,
                triplesToRemoveList),
            cancellationToken);
    }
}
