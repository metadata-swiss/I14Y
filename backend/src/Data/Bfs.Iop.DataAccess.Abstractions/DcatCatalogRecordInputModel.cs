namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogRecordInputModel
{
    public required DcatCatalogResourceModel PrimaryTopic { get; init; }

    public IReadOnlyCollection<DcatCatalogThemeInputModel> Themes { get; init; } = [];
}
