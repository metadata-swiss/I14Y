namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record AnnotationInputModel
{
    public string? Identifier { get; init; }

    public MultiLanguageModel? Text { get; init; }

    public string? Title { get; init; }

    public required string Type { get; init; }

    public string? Uri { get; init; }
}
