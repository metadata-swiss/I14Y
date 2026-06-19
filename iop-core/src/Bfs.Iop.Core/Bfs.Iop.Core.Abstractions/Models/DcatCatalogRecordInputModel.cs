namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogRecordInputModel
{
    public required DcatCatalogResourceModel PrimaryTopic { get; init; }

    public IEnumerable<DcatCatalogThemeInputModel> Themes { get; init; } = [];
}
