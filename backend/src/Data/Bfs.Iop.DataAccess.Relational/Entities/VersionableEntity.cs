namespace Bfs.Iop.DataAccess.Relational.Entities;

internal abstract class VersionableEntity : PublishableEntityBase
{
    public Guid? PreviousVersionId { get; set; }

    public string? Version { get; set; }

    public MultiLanguage? VersionNotes { get; set; }
}

internal abstract class VersionableEntity<TEntity> : VersionableEntity where TEntity : EntityBase
{
    public TEntity? PreviousVersion { get; set; }
}