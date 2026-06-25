using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class ConceptInputCreateVersion
{
    public MultiLanguage Description { get; set; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = [];

    public MultiLanguage Name { get; set; } = default!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid PreviousVersionId { get; set; }

    public Person? ResponsibleDeputy { get; set; }

    public Person ResponsiblePerson { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = default!;
}