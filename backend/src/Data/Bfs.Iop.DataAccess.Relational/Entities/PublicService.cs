namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class PublicService : PublishableEntityBase
{
    public string[] BusinessEvents { get; set; } = [];

    public List<Channel> Channels { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public string[] Identifiers { get; set; } = [];

    public List<PublicServiceIsDescribedAt> IsDescribedAt { get; set; } = [];

    public List<Keyword> Keyword { get; set; } = [];

    public string[] Language { get; set; } = [];

    public string[] LifeEvents { get; set; } = [];

    public List<PublicServiceRelation> Relation { get; set; } = [];

    public List<PublicServiceRequires> Requires { get; set; } = [];

    public Guid? ResponsibleDeputyId { get; set; }

    public IopPerson? ResponsibleDeputy { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public IopPerson? ResponsiblePerson { get; set; }

    public string[] Sector { get; set; } = [];

    public string[] Spatial { get; set; } = [];

    public string[] ThematicArea { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;

    public string[] SpatialCH { get; set; } = [];
}