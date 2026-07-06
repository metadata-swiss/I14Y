namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatDistributionModel
{
    public IEnumerable<IdModel> AccessServices { get; init; } = [];

    public ResourceModel AccessUrl { get; init; } = null!;

    public VocabularyEntryModel? Availability { get; init; }

    public decimal? ByteSize { get; init; }

    public ChecksumModel? Checksum { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<PeriodOfTimeModel> Coverage { get; init; } = [];

    public MultiLanguageModel Description { get; init; } = null!;

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public ResourceModel? DownloadUrl { get; init; }

    public VocabularyEntryModel? Format { get; init; }

    public Guid Id { get; init; }

    public string? Identifier { get; init; }

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<VocabularyEntryModel> Languages { get; init; } = [];

    public VocabularyEntryModel? License { get; init; }

    public VocabularyEntryModel? MediaType { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public VocabularyEntryModel? PackagingFormat { get; init; }

    public string? Rights { get; init; }

    public decimal? SpatialResolution { get; init; }

    public string? TemporalResolution { get; init; }

    public MultiLanguageModel Title { get; init; } = null!;
}
