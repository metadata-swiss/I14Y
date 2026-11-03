namespace Bfs.Iop.DataAccess.Abstractions;

public sealed record DcatCatalogRecordInputModel
{
    public required DcatCatalogResourceModel PrimaryTopic { get; init; }

    /// <summary>
    /// No longer supported: set the themes on the primary topic. Sending any is rejected.
    /// </summary>
    public object? Themes { get; init; }
}
