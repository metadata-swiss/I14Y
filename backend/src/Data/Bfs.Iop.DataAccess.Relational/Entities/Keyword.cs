namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class Keyword : EntityBase
{
    public DataService? DataService { get; set; }

    public Guid? DataServiceId { get; set; }

    public Dataset? Dataset { get; set; }

    public Guid? DatasetId { get; set; }

    public PublicService? PublicService { get; set; }

    public Guid? PublicServiceId { get; set; }

    public IopConcept? IopConcept { get; set; }

    public Guid? IopConceptId { get; set; }

    public MappingTable? MappingTable { get; set; }

    public Guid? MappingTableId { get; set; }

    public MultiLanguage? Text { get; set; }

    public string? Uri { get; set; }
}