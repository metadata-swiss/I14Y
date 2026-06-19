namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatDistributionInputModel
{
    public IEnumerable<IdModel> AccessServices { get; init; } = [];

    public required ResourceModel AccessUrl { get; init; }

    public CodeInputModel? Availability { get; init; }

    public decimal? ByteSize { get; init; }

    public ChecksumInputModel? Checksum { get; init; }

    public IEnumerable<ResourceModel> ConformsTo { get; init; } = [];

    public IEnumerable<PeriodOfTimeModel> Coverage { get; init; } = [];

    public required MultiLanguageModel Description { get; init; }

    public IEnumerable<ResourceModel> Documentation { get; init; } = [];

    public ResourceModel? DownloadUrl { get; init; }

    public CodeInputModel? Format {  get; init; }

    public Guid? Id { get; init; }

    public string? Identifier { get; init; }

    public IEnumerable<ResourceModel> Images { get; init; } = [];

    public DateTimeOffset? Issued { get; init; }

    public IEnumerable<CodeInputModel> Languages { get; init; } = [];

    public CodeInputModel? License { get; init; }

    public CodeInputModel? MediaType { get; init; }

    public DateTimeOffset? Modified { get; init; }

    public CodeInputModel? PackagingFormat { get; init; }

    public string? Rights { get; init; }

    public decimal? SpatialResolution { get; init; }

    public string? TemporalResolution { get; init; }

    public required MultiLanguageModel Title { get; init; }
}