namespace Bfs.Iop.Core.Data.Entities;

internal class DatasetQualityInformation : EntityBase, IReferringDatasetEntity
{
    public DatasetQualityAnswerOption Answer { get; set; } = null!;

    public Guid AnswerId { get; set; }

    public Dataset Dataset { get; set; } = null!;

    public Guid DatasetId { get; set; }

    public string? Detail { get; set; }

    public DatasetQualityQuestion Question { get; set; } = null!;

    public Guid QuestionId { get; set; }
}