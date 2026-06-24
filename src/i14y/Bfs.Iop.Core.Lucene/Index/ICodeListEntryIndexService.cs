using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Lucene.Index;

public interface ICodeListEntryIndexService
{
    global::Lucene.Net.Store.Directory IndexDirectory { get; }

    void Index(IEnumerable<CodeListEntryModel> codeListEntries);

    void UpdateIndex(IEnumerable<CodeListEntryModel> codeListEntries);

    void DeIndex(IEnumerable<Guid> codeListEntriesIds);

    Task BuildIndex(CancellationToken cancellationToken = default);
}
