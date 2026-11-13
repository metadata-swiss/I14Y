namespace Bfs.Iop.DataAccess.Relational.Entities;

internal class DcatCatalog : EntityBase, IOwnedEntity, IMainEntity
{
    public MultiLanguage Description { get; set; } = null!;

    public Guid? PublisherId { get; set; }

    public Agent? Publisher { get; set; }

    public string? ExternalPublisherIdentifier { get; set; }

    public MultiLanguage? ExternalPublisherName { get; set; }

    public MultiLanguage Title { get; set; } = null!;
        
    public Guid TenantId { get; set; }

    public Agent Tenant { get; set; } = null!;

    public string[]? ThemeTaxonomy { get; set; }
}
