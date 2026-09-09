using System.Runtime.CompilerServices;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.IndexSearch.Business.Sources;
using Bfs.Iop.IndexSearch.Contracts.Indexing;

namespace Bfs.Iop.IndexSearch.Data;

internal sealed class CodeListDocumentSource : ICodeListDocumentSource
{
    private const int MaxAncestorDepth = 64;

    private readonly ISearchIndexProviderService _provider;

    public CodeListDocumentSource(ISearchIndexProviderService provider)
    {
        _provider = provider;
    }

    public async IAsyncEnumerable<IReadOnlyList<CodeListIndexDocument>> ReadAllAsync(
        int batchSize,
        [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(batchSize, 1);

        var parents = await ReadParentsAsync(batchSize, cancellationToken);

        await foreach (var batch in _provider.GetCodeListEntriesInBatches(batchSize, cancellationToken))
        {
            yield return [.. batch.Select(x => x.ToIndexDocument(Ancestors(x, parents)))];
        }
    }

    // Codes are unique per concept, not globally, so the map is keyed by both. One pass to build it,
    // one to emit: the same two passes the database walk cost, without the recursive query.
    private async Task<Dictionary<(Guid ConceptId, string Code), string>> ReadParentsAsync(
        int batchSize,
        CancellationToken cancellationToken)
    {
        var parents = new Dictionary<(Guid, string), string>();

        await foreach (var batch in _provider.GetCodeListEntriesInBatches(batchSize, cancellationToken))
        {
            foreach (var entry in batch.Where(x => !string.IsNullOrWhiteSpace(x.ParentCode)))
            {
                parents[(entry.ConceptId, entry.Code)] = entry.ParentCode!;
            }
        }

        return parents;
    }

    // Nearest parent first. Depth-capped because a cycle in the source data would otherwise spin here
    // rather than fail, and a code list nested deeper than this is a data error either way.
    private static IReadOnlyList<string> Ancestors(
        CodeListEntryModel entry,
        Dictionary<(Guid ConceptId, string Code), string> parents)
    {
        if (string.IsNullOrWhiteSpace(entry.ParentCode))
        {
            return [];
        }

        var ancestors = new List<string>();
        var seen = new HashSet<string>(StringComparer.Ordinal);
        var code = entry.ParentCode;

        while (!string.IsNullOrWhiteSpace(code) && seen.Add(code) && ancestors.Count < MaxAncestorDepth)
        {
            ancestors.Add(code);

            if (!parents.TryGetValue((entry.ConceptId, code), out var next))
            {
                break;
            }

            code = next;
        }

        return ancestors;
    }
}