using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CodeListSearchFilter
{
    public IReadOnlyList<CodeListAnnotationCriterion> All { get; init; } = [];

    public IReadOnlyList<CodeListAnnotationCriterion> Any { get; init; } = [];

    public bool IsEmpty => All.Count == 0 && Any.Count == 0;
}

public sealed record CodeListAnnotationCriterion
{
    public required string Type { get; init; }

    public CodeListAnnotationProperty? Property { get; init; }

    public string? Value { get; init; }

    public MultiLanguageModel? Text { get; init; }
}
