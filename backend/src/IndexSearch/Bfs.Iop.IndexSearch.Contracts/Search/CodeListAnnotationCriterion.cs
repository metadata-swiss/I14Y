using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CodeListAnnotationCriterion
{
    public required string Type { get; init; }

    public CodeListAnnotationProperty? Property { get; init; }

    public string? Value { get; init; }

    public MultiLanguageModel? Text { get; init; }
}
