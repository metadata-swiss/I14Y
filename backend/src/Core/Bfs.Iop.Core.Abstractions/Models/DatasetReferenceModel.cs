namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DatasetReferenceModel
{
    public required string Uri { get; init; }

    public Guid? DatasetId { get; init; }

    public MultiLanguageModel? Title { get; init; }

    public MultiLanguageModel? PublisherName { get; init; }
}
