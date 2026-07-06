namespace Bfs.Iop.Core.Data.Entities;

internal class DatasetQualityInformationLink : EntityBase, IReferringDatasetEntity
{
    public Dataset Dataset { get; set; } = null!;

    public Guid DatasetId { get; set; }

    public string Href { get; set; } = null!;

    public MultiLanguage Label { get; set; } = null!;
}