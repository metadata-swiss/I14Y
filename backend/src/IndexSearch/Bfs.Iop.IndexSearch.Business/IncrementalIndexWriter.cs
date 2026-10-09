using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.IndexSearch.Business;

public sealed class IncrementalIndexWriter : IIncrementalIndexWriter
{
    private readonly ICatalogDocumentSource _catalogSource;
    private readonly ICodeListDocumentSource _codeListSource;
    private readonly ICatalogIndexWriter _catalogWriter;
    private readonly ICodeListIndexWriter _codeListWriter;
    private readonly IDatasetStructureSource _structures;
    private readonly ICatalogIndexReader _indexReader;
    private readonly ILogger<IncrementalIndexWriter> _logger;

    public IncrementalIndexWriter(
        ICatalogDocumentSource catalogSource,
        ICodeListDocumentSource codeListSource,
        ICatalogIndexWriter catalogWriter,
        ICodeListIndexWriter codeListWriter,
        IDatasetStructureSource structures,
        ICatalogIndexReader indexReader,
        ILogger<IncrementalIndexWriter> logger)
    {
        _catalogSource = catalogSource ?? throw new ArgumentNullException(nameof(catalogSource));
        _codeListSource = codeListSource ?? throw new ArgumentNullException(nameof(codeListSource));
        _catalogWriter = catalogWriter ?? throw new ArgumentNullException(nameof(catalogWriter));
        _codeListWriter = codeListWriter ?? throw new ArgumentNullException(nameof(codeListWriter));
        _structures = structures ?? throw new ArgumentNullException(nameof(structures));
        _indexReader = indexReader ?? throw new ArgumentNullException(nameof(indexReader));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task UpsertCatalogResourceAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var document = await _catalogSource.ReadOneAsync(type, id, cancellationToken);

        if (document is null)
        {
            _logger.LogInformation(
                "The {Type} '{Id}' no longer exists, so it was removed from the index instead.", type, id);

            await RemoveCatalogResourceAsync(id, cancellationToken);

            return;
        }

        document = await WithStructureFlagAsync(document, cancellationToken);

        var written = await _catalogWriter.WriteAsync([document], cancellationToken);

        if (written != 1)
        {
            throw new InvalidOperationException(
                $"The index rejected the {type} '{id}'.");
        }

        _logger.LogInformation("Indexed the {Type} '{Id}'.", type, id);
    }

    public async Task RemoveCatalogResourceAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deleted = await _catalogWriter.DeleteAsync([id], cancellationToken);

        if (deleted != 1)
        {
            throw new InvalidOperationException(
                $"The index rejected the removal of the catalogue resource '{id}'.");
        }

        _logger.LogInformation("Removed the catalogue resource '{Id}' from the index.", id);
    }

    public async Task ReplaceCodeListAsync(Guid conceptId, CancellationToken cancellationToken = default)
    {
        var documents = await _codeListSource.ReadConceptAsync(conceptId, cancellationToken);

        await _codeListWriter.DeleteByConceptAsync(conceptId, cancellationToken);

        if (documents.Count == 0)
        {
            _logger.LogInformation("The concept '{ConceptId}' has no code list entries left.", conceptId);

            return;
        }

        var written = await _codeListWriter.WriteAsync(documents, cancellationToken);

        if (written != documents.Count)
        {
            throw new InvalidOperationException(
                $"The index accepted {written} of {documents.Count} code list entries for concept '{conceptId}'.");
        }

        _logger.LogInformation(
            "Replaced the code list of the concept '{ConceptId}' with {Written} of {Total} entries.",
            conceptId,
            written,
            documents.Count);
    }

    public async Task RemoveCodeListAsync(Guid conceptId, CancellationToken cancellationToken = default)
    {
        var deleted = await _codeListWriter.DeleteByConceptAsync(conceptId, cancellationToken);

        _logger.LogInformation(
            "Removed {Deleted} code list entries of the concept '{ConceptId}' from the index.",
            deleted,
            conceptId);
    }

    private async Task<CatalogIndexDocument> WithStructureFlagAsync(
        CatalogIndexDocument document,
        CancellationToken cancellationToken)
    {
        if (document.Type != SearchResourceType.Dataset)
        {
            return document;
        }

        var hasStructure = await _structures.HasStructureAsync(document.Id, cancellationToken);

        if (hasStructure is null)
        {
            hasStructure = await CarriedForwardStructureFlagAsync(document.Id, cancellationToken);
        }

        return document with { HasStructure = hasStructure };
    }

    private async Task<bool?> CarriedForwardStructureFlagAsync(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var carried = await _indexReader.ReadStructureFlagAsync(id, cancellationToken);

            _logger.LogWarning(
                "The structure flag of the dataset '{Id}' could not be resolved; carrying the indexed "
                + "value '{Carried}' forward.",
                id,
                carried);

            return carried;
        }
   
        catch (Exception exception) when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogError(
                exception,
                "The structure flag of the dataset '{Id}' could not be resolved and the indexed value "
                + "could not be read either. It is being indexed without one, so it drops out of the "
                + "Structures facet until the next rebuild.",
                id);

            return null;
        }
    }
}
