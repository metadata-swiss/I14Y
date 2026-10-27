using Bfs.Iop.AuditTrail.Abstractions.Models;
using Bfs.Iop.Common.Extensions;

namespace Bfs.Iop.AuditTrail.Business.Extensions;

internal static class ResourceMetadataExtensions
{
    private const string JsonExtension = "json";
    private const string TtlExtension = "ttl";

    public static string GetFilename(this ResourceMetadata resourceMetadata)
    {
        ArgumentNullException.ThrowIfNull(resourceMetadata, nameof(resourceMetadata));

        return $"{resourceMetadata.ResourceType}_{resourceMetadata.Id}.{GetFileExtension(resourceMetadata.ResourceType)}";
    }

    private static string GetFileExtension(AuditTrailResourceType resourceType)
    {
        resourceType.EnsureValueIsValid();

        return resourceType switch
        {
            AuditTrailResourceType.Agent => JsonExtension,
            AuditTrailResourceType.Concept => JsonExtension,
            AuditTrailResourceType.ConceptCodeListEntries => JsonExtension,
            AuditTrailResourceType.DcatCatalog => JsonExtension,
            AuditTrailResourceType.DcatCatalogRecords => JsonExtension,
            AuditTrailResourceType.Dataset => JsonExtension,
            AuditTrailResourceType.DatasetStructure => TtlExtension,
            AuditTrailResourceType.DataService => JsonExtension,
            AuditTrailResourceType.MappingTable => JsonExtension,
            AuditTrailResourceType.MappingTableRelations => JsonExtension,
            AuditTrailResourceType.PublicService => JsonExtension,
            _ => throw new NotSupportedException($"The value '{resourceType}' is not supported."),
        };
    }
}
