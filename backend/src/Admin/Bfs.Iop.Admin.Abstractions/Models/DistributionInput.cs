using Bfs.Iop.DataAccess.Abstractions;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Models;

public sealed class DistributionInput
{
    public AccessUrl AccessUrl { get; set; } = null!;

    public VocabularyEntry? Availability { get; set; }

    public decimal? ByteSize { get; set; }

    public CheckSum? Checksum { get; set; }

    public IEnumerable<Resource> ConformTos { get; set; } = new List<Resource>();

    public IEnumerable<PeriodOfTimeModel> Coverage { get; set; } = [];

    public Guid DatasetId { get; set; }

    public MultiLanguage Description { get; set; } = null!;

    public IEnumerable<Resource> Documents { get; set; } = new List<Resource>();

    public VocabularyEntry? Format { get; set; }

    public Guid Id { get; set; }

    public string? Identifier { get; set; } = null!;

    public IEnumerable<Resource> Image { get; set; } = new List<Resource>();

    public IEnumerable<string> Languages { get; set; } = new List<string>();

    public DateTimeOffset? LastUpdated { get; set; }

    public VocabularyEntry? License { get; set; }

    public VocabularyEntry? MediaType { get; set; }

    public VocabularyEntry? PackagingFormat { get; set; }

    public DateTimeOffset? Published { get; set; }

    public VocabularyEntry? Rights { get; set; }

    public IEnumerable<string> SpatialResolution { get; set; } = new List<string>();

    public string? TemporalResolution { get; set; } = null!;

    public MultiLanguage Title { get; set; } = null!;
}