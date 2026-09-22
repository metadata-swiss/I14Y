namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogRecordModel
{
    public Guid DcatCatalogId { get; init; }

    public Guid Id { get; init; }

    public required DcatCatalogResourceModel PrimaryTopic { get; init; }

    public IReadOnlyCollection<DcatCatalogThemeModel> Themes { get; init; } = [];
}
