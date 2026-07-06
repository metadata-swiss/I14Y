namespace Bfs.Iop.Core.Data.Entities;

internal sealed class AgentSubAgentRelation : EntityBase
{
    public Guid AgentId { get; set; }

    public Agent? Agent { get; set; }

    public Guid SubAgentId { get; set; }

    public Agent? SubAgent { get; set; }
}
