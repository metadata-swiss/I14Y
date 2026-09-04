namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetQualityQuestionModel
{
    public IReadOnlyCollection<DatasetQualityAnswerOptionModel> AnswerOptions { get; set; } = [];

    public Guid Id { get; init; }

    public bool IsMandatory { get; init; }

    public int Order { get; init; }

    public required MultiLanguageModel Question { get; init; } 
}
