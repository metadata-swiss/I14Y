using System;
using System.Runtime.Serialization;

namespace Bfs.Iop.Admin.Models;

public class CodelistEntryInput : ICodeListEntryInputModel
{
    [Obsolete]
    public Guid? CodelistId { get; set; }

    public MultiLanguage? Description { get; set; }

    public Guid Id { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public string? ParentCode { get; set; }

    public string Value { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public Guid? ConceptId { get; set; }
}