namespace Bfs.Iop.Core.Abstractions.Models.Indexing;

/// <summary>
/// A code-list entry flattened into exactly what the code-list index stores.
/// <para>
/// The code-list twin of <see cref="CatalogIndexEntry"/>, and produced the same two ways: projected
/// from EF for a rebuild, projected from a domain model for a single write.
/// </para>
/// </summary>
public sealed record CodeListIndexEntry
{
    public required Guid Id { get; init; }

    public required Guid ConceptId { get; init; }

    public required string Code { get; init; }

    /// <summary>
    /// The parent entry's <b>code</b>, not its id.
    /// <para>
    /// Worth stating because the entity holds only a <c>ParentCodeListEntryId</c> foreign key, so the
    /// EF projection has to join the parent row to fill this in. Leave it null when there is no
    /// parent — but leaving it null when there IS one silently flattens the code-list hierarchy in
    /// search, with no error anywhere.
    /// </para>
    /// </summary>
    public string? ParentCode { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public IReadOnlyList<IndexAnnotation> Annotations { get; init; } = [];
}

/// <summary>
/// One annotation on a code-list entry. Indexed as a <c>nested</c> document so a filter matches a
/// field and its value together, rather than any field against any value.
/// </summary>
public sealed record IndexAnnotation
{
    public required string Type { get; init; }

    public string? Identifier { get; init; }

    public string? Title { get; init; }

    public string? Uri { get; init; }

    public MultiLanguageModel? Text { get; init; }
}
