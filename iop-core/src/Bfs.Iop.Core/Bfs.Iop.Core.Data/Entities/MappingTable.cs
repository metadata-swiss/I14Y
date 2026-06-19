namespace Bfs.Iop.Core.Data.Entities;

internal class MappingTable : PublishableEntityBase
{
    public ICollection<Resource> ConformsTo { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public string[] Identifiers { get; set; } = [];

    public ICollection<Keyword> Keywords { get; set; } = [];

    public MultiLanguage Name { get; set; } = null!;

    public ICollection<MappingRelation> Relations { get; set; } = [];

    public IopPerson? ResponsibleDeputy { get; set; }

    public Guid? ResponsibleDeputyId { get; set; }

    public IopPerson ResponsiblePerson { get; set; } = null!;

    public Guid ResponsiblePersonId { get; set; }

    public string SourceUri { get; set; } = null!;

    public string TargetUri { get; set; } = null!;

    public string[] Themes { get; set; } = [];

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = null!;
}
