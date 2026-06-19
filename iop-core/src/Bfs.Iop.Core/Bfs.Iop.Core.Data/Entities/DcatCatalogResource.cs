namespace Bfs.Iop.Core.Data.Entities;

internal class DcatCatalogResource : EntityBase
{
    public Guid ResourceId { get; set; }

    public string ResourceType { get; set; } = null!;

    public DcatCatalogRecord DcatCatalogRecord { get; set; } = null!;

    public Guid DcatCatalogRecordId { get; set; }
}