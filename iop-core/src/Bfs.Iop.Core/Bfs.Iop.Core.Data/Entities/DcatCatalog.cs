namespace Bfs.Iop.Core.Data.Entities;

internal class DcatCatalog : EntityBase, IOwnedEntity, IMainEntity
{
    public MultiLanguage Description { get; set; } = null!;

    public MultiLanguage Title { get; set; } = null!;
        
    public Guid PublisherId { get; set; }

    public Agent Publisher { get; set; } = null!;

    public string[]? ThemeTaxonomy { get; set; }
}
