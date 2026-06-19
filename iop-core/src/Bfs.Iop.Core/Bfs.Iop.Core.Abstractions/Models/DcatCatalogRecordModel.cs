namespace Bfs.Iop.Core.Abstractions.Models;

public sealed record DcatCatalogRecordModel
{
    public Guid DcatCatalogId { get; init; }

    public Guid Id { get; init; }

    public required DcatCatalogResourceModel PrimaryTopic { get; init; }

    public IEnumerable<DcatCatalogThemeModel> Themes { get; init; } = [];
}
