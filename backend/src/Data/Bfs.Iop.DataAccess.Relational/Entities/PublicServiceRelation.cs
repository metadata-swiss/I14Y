namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class PublicServiceRelation : EntityBase
{
    public PublicService PublicService { get; set; } = null!;

    public Guid PublicServiceId { get; set; }

    public PublicService Relation { get; set; } = null!;

    public Guid RelationId { get; set; }
}