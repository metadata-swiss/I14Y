using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Messaging.SearchIndex;

internal enum SearchIndexTarget
{
    CatalogResource = 1,
    CodeList = 2,
}

internal enum SearchIndexOperation
{
    Upsert = 1,
    Remove = 2,
}

/// <summary>
///     What changed, not what it now contains.
/// </summary>
internal sealed record SearchIndexMessage(
    SearchIndexTarget Target,
    Guid Id,
    SearchIndexOperation Operation,
    SearchResourceType? ResourceType = null);
