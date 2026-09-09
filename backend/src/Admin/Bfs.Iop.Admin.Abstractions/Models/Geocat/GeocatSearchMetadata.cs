using System;

namespace Bfs.Iop.Admin.Models.Geocat;

public sealed class GeocatSearchMetadata
{
    public MultilingualText? Abstract { get; set; }

    public string? Identifier { get; set; }

    public Uri? Link { get; set; }

    public MultilingualText? Title { get; set; }
}