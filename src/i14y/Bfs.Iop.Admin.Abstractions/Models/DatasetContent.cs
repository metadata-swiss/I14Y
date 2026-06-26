using System;

namespace Bfs.Iop.Admin.Models;

public class DatasetContent
{
    public MultiLanguage? Agency { get; set; } = null!;

    public Guid Id { get; set; }

    public string Identifier { get; set; } = null!;

    public MultiLanguage Name { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string Type { get; set; } = null!;

    public DateTimeOffset? ValidFrom { get; set; }

    public DateTimeOffset? ValidTo { get; set; }

    public string Version { get; set; } = null!;
}