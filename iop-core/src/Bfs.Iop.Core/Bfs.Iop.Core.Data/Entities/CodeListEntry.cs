namespace Bfs.Iop.Core.Data.Entities;

internal sealed class CodeListEntry : EntityBase
{
    public ICollection<Annotation> Annotations { get; set; } = [];

    public string Code { get; set; } = null!;

    public MultiLanguage? Description { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public IopConcept IopConcept { get; set; } = null!;

    public Guid IopConceptId { get; set; }

    public CodeListEntry? ParentCodeListEntry { get; set; }

    public Guid? ParentCodeListEntryId { get; set; }

    public int Position { get; set; }

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }
}
