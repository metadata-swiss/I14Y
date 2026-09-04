namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class DatasetQualityAnswerOption : EntityBase
{
    public Guid DatasetQualityQuestionId { get; set; }

    public MultiLanguage Detail { get; set; } = null!;

    public bool DetailMandatory { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public List<DatasetQualityInformation> QualityInformations { get; set; } = null!;

    public DatasetQualityQuestion Question { get; set; } = null!;

    public string Value { get; set; } = null!;
}