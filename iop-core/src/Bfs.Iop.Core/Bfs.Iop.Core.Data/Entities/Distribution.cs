namespace Bfs.Iop.Core.Data.Entities;

internal sealed class Distribution : EntityBase
{
    public List<Resource> AccessUrl { get; set; } = [];

    public string? AvailabilityVocabulary { get; set; }

    public decimal? ByteSize { get; set; }

    public CheckSum? Checksum { get; set; }

    public List<Resource> ConformsTo { get; set; } = [];

    public List<PeriodOfTime> Coverage { get; set; } = [];

    public Dataset? Dataset { get; set; }

    public Guid? DatasetId { get; set; }

    public MultiLanguage Description { get; set; } = null!;

    public List<Resource> Documentation { get; set; } = [];

    public List<Resource> DownloadUrl { get; set; } = [];

    public string? Format { get; set; }

    public string? Identifier { get; set; }

    public List<Resource> Image { get; set; } = [];

    public DateTimeOffset? Issued { get; set; }

    public string[] Language { get; set; } = [];

    public string? License { get; set; }

    public string? MediaType { get; set; }

    public DateTimeOffset? Modified { get; set; }

    public string? PackagingFormat { get; set; }

    public string? Rights { get; set; }

    public string[] SpatialResolution { get; set; } = [];

    public string? TemporalResolution { get; set; }

    public MultiLanguage Title { get; set; } = null!;

    public List<DistributionDataServiceRelation> AccessServices { get; set; } = [];
}