using System;

namespace Bfs.Iop.Admin.Models;

public class CodelistEntryInput : ICodeListEntryInputModel
{
    public MultiLanguage? Description { get; set; }

    public Guid Id { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public string? ParentCode { get; set; }

    public string Value { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public Guid? ConceptId { get; set; }
}