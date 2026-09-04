namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record CodeListEntryModel
{
    public IReadOnlyCollection<AnnotationModel>? Annotations { get; init; } = [];

    public required string Code { get; init; }

    public required Guid ConceptId { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public Guid Id { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public string? ParentCode { get; init; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }
}
