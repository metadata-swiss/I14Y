namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class DistributionDataServiceRelation : EntityBase
{
    public Guid DistributionId { get; set; }

    public Distribution? Distribution { get; set; }

    public Guid DataServiceId { get; set; }

    public DataService? DataService { get; set; }
}
