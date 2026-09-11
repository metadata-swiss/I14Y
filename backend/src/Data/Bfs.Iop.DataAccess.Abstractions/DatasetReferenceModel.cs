namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetReferenceModel
{
    public required string Uri { get; init; }

    public Guid? DatasetId { get; init; }

    public MultiLanguageModel? Title { get; init; }

    public MultiLanguageModel? PublisherName { get; init; }
}
