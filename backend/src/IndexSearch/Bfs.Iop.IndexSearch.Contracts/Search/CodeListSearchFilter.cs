using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

// A code list's filters are configured per concept, so the shapes they can take are open-ended. This
// is the neutral form: whoever holds the configuration translates it into criteria, and the engine
// turns criteria into a query without knowing what a "filter" means to the front-end.
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
