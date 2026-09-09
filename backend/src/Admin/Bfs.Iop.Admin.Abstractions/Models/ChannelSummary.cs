using System;

using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class ChannelSummary
{
    public Guid Id { get; set; }

    public string Identifier { get; set; } = null!;

    public IEnumerable<MultiLanguage> OwnedBy { get; set; } = null!;

    public MultiLanguage? Type { get; set; } = null!;
}