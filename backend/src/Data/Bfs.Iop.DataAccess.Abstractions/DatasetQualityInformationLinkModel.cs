namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DatasetQualityInformationLinkModel
{
    public string? Href { get; init; }

    public Guid Id { get; set; }

    public MultiLanguageModel? Label { get; init; }
}
