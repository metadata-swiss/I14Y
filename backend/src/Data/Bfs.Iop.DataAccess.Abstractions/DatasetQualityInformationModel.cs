namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetQualityInformationModel
{
    public Guid AnswerId { get; init; }

    public string? Detail { get; init; }

    public Guid Id { get; init; }

    public Guid QuestionId { get; init; }
}
