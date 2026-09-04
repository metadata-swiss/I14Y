namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record CodeListEntryInputModel
{
    public IReadOnlyCollection<AnnotationInputModel> Annotations { get; set; } = [];

    public required string Code { get; set; }

    public MultiLanguageModel? Description { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public string? ParentCode { get; set; }

    public DateTimeOffset? ValidFrom { get; init; }

    public DateTimeOffset? ValidTo { get; init; }
}
