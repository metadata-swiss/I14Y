namespace Bfs.Iop.Search.Abstractions;

/// <summary>
/// Rebuilds one index in full from the system of record.
/// </summary>
public interface IIndexBuilderService
{
    Task BuildIndexAsync(CancellationToken cancellationToken = default);
}
