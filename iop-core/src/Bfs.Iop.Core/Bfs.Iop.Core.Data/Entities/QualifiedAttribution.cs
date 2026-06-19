namespace Bfs.Iop.Core.Data.Entities;

internal class QualifiedAttribution : EntityBase
{
    public Agent Agent { get; set; } = null!;

    public Guid? AgentId { get; set; }

    public Dataset Dataset { get; set; } = null!;

    public Guid? DatasetId { get; set; }

    public string HadRole { get; set; } = string.Empty;
}