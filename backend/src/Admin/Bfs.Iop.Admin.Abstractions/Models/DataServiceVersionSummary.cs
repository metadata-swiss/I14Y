using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Bfs.Iop.Admin.Models;

public class DataServiceVersionSummary
{
    public IEnumerable<Resource> EndpointUrls { get; set; } = null!;

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public Guid Id { get; set; }

    public MultiLanguage Title { get; set; } = null!;

    public string Version { get; set; } = null!;
}