using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.IndexSearch.Contracts.Search;

public sealed record CodeListSearchHit
{
    public required Guid Id { get; init; }

    public required Guid ConceptId { get; init; }

    public required string Code { get; init; }

    public string? ParentCode { get; init; }

    public MultiLanguageModel? Name { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public IReadOnlyList<AnnotationInputModel> Annotations { get; init; } = [];

    public IReadOnlyList<string> AncestorCodes { get; init; } = [];
}
