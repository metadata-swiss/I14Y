using System;

namespace Bfs.Iop.Admin.Models.OpenData;

public sealed class OpenDataSearchResultItem
{
    public MultilingualText? Description { get; set; }

    public string? Identifier { get; set; }

    public Uri? Link { get; set; }

    public MultilingualText? Title { get; set; }
}