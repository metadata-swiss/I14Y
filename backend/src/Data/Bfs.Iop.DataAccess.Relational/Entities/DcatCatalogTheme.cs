namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class DcatCatalogTheme : EntityBase
{
    public string Code { get; set; } = string.Empty;

    public DcatCatalogRecord DcatCatalogRecord { get; set; } = null!;

    public Guid DcatCatalogRecordId { get; set; }

    public string ThemeTaxonomy { get; set; } = string.Empty;
}