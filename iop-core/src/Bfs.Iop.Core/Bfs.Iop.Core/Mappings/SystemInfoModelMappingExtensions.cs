using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Lucene.Search;

namespace Bfs.Iop.Core.Mappings;

internal static class SystemInfoModelMappingExtensions
{
    public static SystemInfoModel MapSystemInfoToSystemInfoModel(this EntityBase entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            CreatedAt = entity.CreatedAt,
            CreationType = entity.CreationType,
            ModifiedAt = entity.ModifiedAt,
        };
    }

    public static SystemInfoModel MapToSystemInfoModel(this CatalogSearchResultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));

        return new()
        {
            CreatedAt = entry.CreatedAt,
            CreationType = entry.CreationType,
            ModifiedAt = entry.ModifiedAt,
        };
    }
}
