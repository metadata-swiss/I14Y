namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// What a full index build managed to include.
/// </summary>
/// <param name="StructuresAvailable">
/// Whether the dataset structures container could be listed. When false, every dataset was indexed
/// as having no structure, so the Structures facet is empty and the <c>structure</c> filter matches
/// nothing. The build still succeeds — a missing facet is better than no index — but the caller must
/// be able to say so, because otherwise the only symptom is a filter that looks broken.
/// </param>
public readonly record struct IndexBuildReport(bool StructuresAvailable);

/// <summary>
/// Rebuilds one index in full from the system of record.
/// </summary>
public interface IIndexBuilderService
{
    Task<IndexBuildReport> BuildIndexAsync(CancellationToken cancellationToken = default);
}
