namespace Bfs.Iop.Core.Data.Entities;

internal class MappingRelation : EntityBase
{
    public MappingTable MappingTable { get; set; } = null!;

    public Guid MappingTableId { get; set; }

    public string SourceCodeUri { get; set; } = null!;

    public string TargetCodeUri { get; set; } = null!;

    public string RelationType { get; set; } = null!;
}
