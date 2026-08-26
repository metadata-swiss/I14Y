using Bfs.Iop.AuditTrail.Abstractions.Models;

namespace Bfs.Iop.AuditTrail.Business.Extensions;

internal static class ResourceMetadataExtensions
{
    public static string GetFilename(this ResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resourceMetadata, nameof(resourceMetadata));

        return $"{resourceMetadata.Identifier}_{resourceMetadata.Id}.{resourceMetadata.DataFormat}";
    }
}
