using Bfs.Iop.Core.Messaging.SearchIndex;
using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal static class DcatCatalogSearchIndexExtensions
{
    public static async Task NotifyDcatCatalogResourcesChangedAsync(
        this ISearchIndexNotifierService searchIndexNotifier,
        IEnumerable<DcatCatalogResourceModel> resources,
        CancellationToken cancellationToken)
    {
        foreach (var resource in resources.Distinct())
        {
            await searchIndexNotifier.NotifyResourceChangedAsync(
                resource.ResourceType.ToSearchResourceType(),
                resource.ResourceId,
                cancellationToken);
        }
    }

    private static SearchResourceType ToSearchResourceType(this DcatCatalogType type) => type switch
    {
        DcatCatalogType.Dataset => SearchResourceType.Dataset,
        DcatCatalogType.DataService => SearchResourceType.DataService,
        _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
    };
}
