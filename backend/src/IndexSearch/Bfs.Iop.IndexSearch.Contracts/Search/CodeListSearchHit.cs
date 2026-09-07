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

    // AnnotationInputModel rather than AnnotationModel: the latter carries Id and CodeListEntryId,
    // and row identity has no business on a search response.
    //
    // Annotations are indexed as nested documents, so the code-list index holds several times more
    // Lucene documents than entries: 3.4 million against 490,754. Read entry counts with _count,
    // never with _cat/indices.
    public IReadOnlyList<AnnotationInputModel> Annotations { get; init; } = [];

    public IReadOnlyList<string> AncestorCodes { get; init; } = [];
}
