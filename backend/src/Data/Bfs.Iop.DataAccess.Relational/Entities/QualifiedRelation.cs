namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class QualifiedRelation : EntityBase
{
    public Dataset? Dataset { get; set; }

    public Guid? DatasetId { get; set; }

    public string HadRole { get; set; } = string.Empty;

    public Resource Relation { get; set; } = new();
}