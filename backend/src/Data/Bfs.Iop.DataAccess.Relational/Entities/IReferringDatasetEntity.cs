namespace Bfs.Iop.DataAccess.Relational.Entities;

internal interface IReferringDatasetEntity
{
    internal Dataset Dataset { get; set; }

    internal Guid DatasetId { get; set; }
}