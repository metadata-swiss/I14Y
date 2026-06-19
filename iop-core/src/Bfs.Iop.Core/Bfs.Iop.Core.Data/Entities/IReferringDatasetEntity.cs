namespace Bfs.Iop.Core.Data.Entities;

internal interface IReferringDatasetEntity
{
    internal Dataset Dataset { get; set; }

    internal Guid DatasetId { get; set; }
}