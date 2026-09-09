using Bfs.Iop.DataAccess.Abstractions;
using System;

namespace Bfs.Iop.Admin.Models;

public class DcatCatalog
{
    public MultiLanguage Description { get; set; } = new();

    public Guid Id { get; set; }

    public Agent Publisher { get; set; } = new();

    public string[]? ThemeTaxonomy { get; set; }

    public MultiLanguage Title { get; set; } = new();

    public SystemInfoModel? System { get; set; }
}