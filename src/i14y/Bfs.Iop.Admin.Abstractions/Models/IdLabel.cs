using System;

namespace Bfs.Iop.Admin.Models;

public class IdLabel
{
    public Guid Id { get; set; }

    public MultiLanguage Label { get; set; } = null!;
}