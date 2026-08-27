namespace Bfs.Iop.Core.IndexForwarding;

/// <summary>Which IndexSearch endpoint a queued item belongs to.</summary>
public enum IndexForwardTarget
{
    /// <summary>Catalog resources of any kind — the entry carries its own type.</summary>
    Catalog,

    CodeListEntries,

    /// <summary>Removal from the catalog index, by id.</summary>
    CatalogDelete,

    /// <summary>Removal from the code-list-entry index, by id.</summary>
    CodeListEntryDelete,
}

/// <summary>
/// One resource waiting to be forwarded to the IndexSearch service.
/// <para>
/// Index items carry the model itself, so the receiving service never has to read it back — which
/// also keeps it clear of the read-authorization filter that would otherwise drop every non-public
/// resource from the index. Delete items carry only the id, because the row no longer exists.
/// </para>
/// </summary>
public sealed record IndexForwardItem
{
    public required IndexForwardTarget Target { get; init; }

    /// <summary>The model to index; null for the two delete targets.</summary>
    public object? Model { get; init; }

    /// <summary>The id to remove; null for the index targets.</summary>
    public Guid? Id { get; init; }
}
