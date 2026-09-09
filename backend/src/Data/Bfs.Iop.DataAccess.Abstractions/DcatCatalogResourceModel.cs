namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogResourceModel
{
    public Guid ResourceId { get; init; }

    public DcatCatalogType ResourceType { get; init; }
}
