namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatDistributionInputModel
{
    public IReadOnlyCollection<IdModel> AccessServices { get; init; } = [];

    public required ResourceModel AccessUrl { get; init; }

    public CodeInputModel? Availability { get; init; }

    public decimal? ByteSize { get; init; }

    public ChecksumInputModel? Checksum { get; init; }

    public IReadOnlyCollection<ResourceModel> ConformsTo { get; init; } = [];

    public IReadOnlyCollection<PeriodOfTimeModel> Coverage { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IReadOnlyCollection<ResourceModel> Documentation { get; init; } = [];

    public ResourceModel? DownloadUrl { get; init; }

    public CodeInputModel? Format {  get; init; }

    public Guid? Id { get; init; }

    public string? Identifier { get; init; }

    public IReadOnlyCollection<ResourceModel> Images { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IReadOnlyCollection<CodeInputModel> Languages { get; init; } = [];

    public CodeInputModel? License { get; init; }

    public CodeInputModel? MediaType { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public CodeInputModel? PackagingFormat { get; init; }

    public string? Rights { get; init; }

    public decimal? SpatialResolution { get; init; }

    public string? TemporalResolution { get; init; }

    public required MultiLanguageModel Title { get; init; }
}