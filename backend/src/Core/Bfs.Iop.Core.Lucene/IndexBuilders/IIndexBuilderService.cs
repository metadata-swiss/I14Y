namespace Bfs.Iop.Core.Lucene.IndexBuilders;

public interface IIndexBuilderService
{
    Task BuildIndex(CancellationToken cancellationToken = default);
}
