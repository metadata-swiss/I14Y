using System.Runtime.CompilerServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class CatalogDocumentSource : ICatalogDocumentSource
{
    private readonly ISearchIndexProviderService _provider;

    public CatalogDocumentSource(ISearchIndexProviderService provider)
    {
        _provider = provider;
    }

    public async IAsyncEnumerable<IReadOnlyList<CatalogIndexDocument>> ReadAllAsync(
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        await foreach (var batch in _provider.GetDatasetsInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument())];
        }

        await foreach (var batch in _provider.GetDataServicesInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument())];
        }

        await foreach (var batch in _provider.GetPublicServicesInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument())];
        }

        await foreach (var batch in _provider.GetIopConceptsInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument())];
        }

        await foreach (var batch in _provider.GetMappingTablesInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument())];
        }
    }

    public async Task<CatalogIndexDocument?> ReadOneAsync(
        SearchResourceType type,
        Guid id,
        CancellationToken cancellationToken = default) => type switch
        {
            SearchResourceType.Dataset =>
                (await _provider.GetDatasetById(id, cancellationToken))?.ToIndexDocument(),
            SearchResourceType.DataService =>
                (await _provider.GetDataServiceById(id, cancellationToken))?.ToIndexDocument(),
            SearchResourceType.PublicService =>
                (await _provider.GetPublicServiceById(id, cancellationToken))?.ToIndexDocument(),
            SearchResourceType.Concept =>
                (await _provider.GetIopConceptById(id, cancellationToken))?.ToIndexDocument(),
            SearchResourceType.MappingTable =>
                (await _provider.GetMappingTableById(id, cancellationToken))?.ToIndexDocument(),
            _ => throw new NotSupportedException($"The resource type '{type}' cannot be indexed."),
        };
}