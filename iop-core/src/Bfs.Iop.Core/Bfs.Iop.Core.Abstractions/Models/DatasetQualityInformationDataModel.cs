namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DatasetQualityInformationDataModel
{
    public Guid DatasetId { get; init; }

    public IEnumerable<DatasetQualityInformationLinkModel> Documentation { get; init; } = [];

    public IEnumerable<DatasetQualityInformationModel> QualityInformations { get; init; } = [];
}
