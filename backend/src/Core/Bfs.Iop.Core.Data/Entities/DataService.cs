namespace Bfs.Iop.Core.Data.Entities;

internal class DataService : VersionableEntity<DataService>
{
    public string AccessRights { get; set; } = null!;

    public List<Resource> ConformsTo { get; set; } = [];

    public List<VCard> ContactPoint { get; set; } = [];

    public List<DataServiceDataset> Datasets { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public List<Resource> Documentation { get; set; } = [];

    public List<Resource> EndpointDescription { get; set; } = [];

    public List<Resource> EndpointUrl { get; set; } = [];

    public string[] Identifiers { get; set; } = [];

    public DateTimeOffset? Issued { get; set; }

    public List<Keyword> Keyword { get; set; } = [];

    public List<Resource> LandingPage { get; set; } = [];

    public string? License { get; set; }

    public DateTimeOffset? Modified { get; set; }

    public IopPerson? ResponsiblePerson { get; set; }

    public Guid? ResponsiblePersonId { get; set; }

    public IopPerson? ResponsibleDeputy { get; set; }

    public Guid? ResponsibleDeputyId { get; set; }

    public string[] Theme { get; set; } = [];

    public MultiLanguage Title { get; set; } = null!;
}