namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class PublicServiceRequires : EntityBase
{
    public PublicService PublicService { get; set; } = null!;

    public Guid PublicServiceId { get; set; }

    public PublicService Requires { get; set; } = null!;

    public Guid RequiresId { get; set; }
}