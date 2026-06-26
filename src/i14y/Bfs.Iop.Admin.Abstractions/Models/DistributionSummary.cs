using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public class DistributionSummary
{
    public decimal? ByteSize { get; set; }

    public VocabularyEntry? Format { get; set; }

    public Guid Id { get; set; }

    public IEnumerable<string> Languages { get; set; } = new List<string>();

    public DateTimeOffset? LastUpdated { get; set; }

    public DateTimeOffset? Published { get; set; }

    public MultiLanguage Title { get; set; } = null!;
}