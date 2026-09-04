namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class DataServiceDataset : EntityBase
{
    public DataService? DataService { get; set; }

    public Guid DataServiceId { get; set; }

    public Dataset? Dataset { get; set; }

    public Guid DatasetId { get; set; }
}