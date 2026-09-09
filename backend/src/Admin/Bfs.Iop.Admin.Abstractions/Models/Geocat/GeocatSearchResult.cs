using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models.Geocat;

public sealed class GeocatSearchResult
{
    public int Count { get; set; }

    public int From { get; set; }

    public ICollection<GeocatSearchMetadata> Metadata { get; set; } = [];

    public int To { get; set; }
}