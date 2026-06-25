using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class DatasetVersionSummary
{
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public IEnumerable<string> Identifiers { get; set; } = new List<string>();

    public MultiLanguage Title { get; set; } = null!;

    public string Version { get; set; } = null!;
}