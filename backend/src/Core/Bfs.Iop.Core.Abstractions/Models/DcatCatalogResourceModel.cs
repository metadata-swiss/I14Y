namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogResourceModel
{
    public Guid ResourceId { get; init; }

    public DcatCatalogType ResourceType { get; init; }
}
