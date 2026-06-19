namespace Bfs.Iop.Core.Data.Entities;

internal class PublicServiceIsDescribedAt : EntityBase
{
    public Dataset IsDescribedAt { get; set; } = null!;

    public Guid IsDescribedAtId { get; set; }

    public PublicService PublicService { get; set; } = null!;

    public Guid PublicServiceId { get; set; }
}