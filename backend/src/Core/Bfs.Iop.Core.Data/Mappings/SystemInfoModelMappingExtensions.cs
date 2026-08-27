using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

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

}
