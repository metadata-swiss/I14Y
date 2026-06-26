using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class CodeListEntryDetail : CodelistEntryInput
{
    public IEnumerable<Annotation> Annotations { get; set; } = [];

    public bool HasChildren { get; set; }
}