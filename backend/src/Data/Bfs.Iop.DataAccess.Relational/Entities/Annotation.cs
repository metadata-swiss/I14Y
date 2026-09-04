namespace Bfs.Iop.DataAccess.Relational.Entities;

internal sealed class Annotation : EntityBase
{
    public CodeListEntry CodeListEntry { get; set; } = null!;

    public Guid CodeListEntryId { get; set; }

    public string? Identifier { get; set; }

    public int Position { get; set; }

    public MultiLanguage? Text { get; set; }

    public string? Title { get; set; }

    public string Type { get; set; } = null!;

    public string? Uri { get; set; }
}
