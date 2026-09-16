namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record AnnotationModel : IReadOnlyModel
{
    public Guid Id { get; init; }

    public required Guid CodeListEntryId { get; init; }

    public string? Identifier { get; init; }    

    public MultiLanguageModel? Text { get; init; }

    public string? Title { get; init; }

    public required string Type { get; init; }

    public string? Uri { get; init; }
}