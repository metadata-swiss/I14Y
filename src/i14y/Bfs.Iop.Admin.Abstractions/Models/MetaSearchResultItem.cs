using System;

namespace Bfs.Iop.Admin.Models;

public sealed class MetaSearchResultItem
{
    public LocalizedText? Description { get; set; }

    public string? Identifier { get; set; }

    public Uri? Link { get; set; }

    public LocalizedText? Title { get; set; }
}
