namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class Agent : EntityBase, IMainEntity
{
    public string? Classification { get; set; }

    public VCard? ContactPoint { get; set; }

    public MultiLanguage? Description { get; set; }

    public string? HomePage { get; set; }

    public string Identifier { get; set; } = null!;

    public List<Resource> Images { get; set; } = [];

    public MultiLanguage Name { get; set; } = null!;

    public MultiLanguage PrefLabel { get; set; } = null!;

    public string[] Spatial { get; set; } = [];

    public string[] SpatialCH { get; set; } = [];

    public ICollection<AgentSubAgentRelation> SubAgents { get; set; } = [];

    public string? Uid { get; set; }
}