namespace Bfs.Iop.Core.Data.Entities;

internal class DcatCatalogRecord : EntityBase
{
    public DcatCatalog DcatCatalog { get; set; } = null!;

    public Guid DcatCatalogId { get; set; }

    public DcatCatalogResource PrimaryTopic { get; set; } = null!;

    // ToDo: Make this themes disappear
    public List<DcatCatalogTheme> Themes { get; set; } = [];
}