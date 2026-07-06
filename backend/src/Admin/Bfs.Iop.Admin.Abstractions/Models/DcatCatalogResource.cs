using System;

namespace Bfs.Iop.Admin.Models;

public class DcatCatalogResource
{
    public Guid Id { get; set; }

    public Guid ResourceId { get; set; }

    public string ResourceType { get; set; } = string.Empty;
}