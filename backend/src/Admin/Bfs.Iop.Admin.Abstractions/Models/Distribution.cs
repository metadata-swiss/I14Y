using Bfs.Iop.Core.Abstractions.Models;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class Distribution : DistributionSummary
{
    public IEnumerable<Resource> AccessUrls { get; set; } = new List<Resource>();

    public VocabularyEntry? Availability { get; set; } = null!;

    public CheckSum? Checksum { get; set; } = null!;

    public IEnumerable<Resource> ConformTos { get; set; } = new List<Resource>();

    public IEnumerable<PeriodOfTimeModel> Coverage { get; set; } = [];

    public MultiLanguage Description { get; set; } = null!;

    public IEnumerable<Resource> Documentation { get; set; } = new List<Resource>();

    public IEnumerable<Resource> DownloadUrls { get; set; } = new List<Resource>();

    public string? Identifier { get; set; }

    public IEnumerable<Resource> Image { get; set; } = new List<Resource>();

    public VocabularyEntry? License { get; set; }

    public VocabularyEntry? MediaType { get; set; }

    public VocabularyEntry? PackagingFormat { get; set; }

    public VocabularyEntry? Rights { get; set; }

    public IEnumerable<string> SpatialResolution { get; set; } = new List<string>();

    public string? TemporalResolution { get; set; }
}