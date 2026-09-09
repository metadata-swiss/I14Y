namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetQualityInformationDataModel
{
    public Guid DatasetId { get; init; }

    public IReadOnlyCollection<DatasetQualityInformationLinkModel> Documentation { get; init; } = [];

    public IReadOnlyCollection<DatasetQualityInformationModel> QualityInformations { get; init; } = [];
}
