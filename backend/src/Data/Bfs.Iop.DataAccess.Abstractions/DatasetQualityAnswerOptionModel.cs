namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetQualityAnswerOptionModel
{
    public MultiLanguageModel? Detail { get; init; }

    public bool IsDetailMandatory { get; init; }

    public Guid Id { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public string? Value { get; init; }
}
