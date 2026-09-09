using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

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
