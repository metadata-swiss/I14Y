using System;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class CodelistInput
{
    public int? CodelistEntryValueMaxLength { get; set; }

    public MultiLanguage Description { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public string Identifier { get; set; } = null!;

    public bool IsNumeric { get; set; }

    public MultiLanguage Name { get; set; } = null!;

    public Person ResponsibleDeputy { get; set; } = null!;

    public Person ResponsiblePerson { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = null!;
}