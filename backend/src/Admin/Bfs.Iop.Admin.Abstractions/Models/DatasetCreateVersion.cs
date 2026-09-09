using System;

namespace Bfs.Iop.Admin.Models;

public class DatasetCreateVersion
{
    public MultiLanguage Description { get; set; } = null!;

    public string Identifier { get; set; } = null!;

    public Guid PreviousVersionId { get; set; }

    public MultiLanguage Title { get; set; } = null!;

    public string Version { get; set; } = null!;

    public MultiLanguage VersionNotes { get; set; } = null!;
}