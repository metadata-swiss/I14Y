namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CodeListSearchFilter
{
    public IReadOnlyList<CodeListAnnotationCriterion> All { get; init; } = [];

    public IReadOnlyList<CodeListAnnotationCriterion> Any { get; init; } = [];

    public bool IsEmpty => All.Count == 0 && Any.Count == 0;
}
