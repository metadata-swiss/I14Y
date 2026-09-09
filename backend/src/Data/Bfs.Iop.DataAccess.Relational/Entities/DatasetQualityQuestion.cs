namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class DatasetQualityQuestion : EntityBase
{
    public List<DatasetQualityAnswerOption> AnswerOptions { get; set; } = null!;

    public bool Mandatory { get; set; }

    public int Order { get; set; }

    public List<DatasetQualityInformation> QualityInformations { get; set; } = null!;

    public MultiLanguage Question { get; set; } = null!;
}