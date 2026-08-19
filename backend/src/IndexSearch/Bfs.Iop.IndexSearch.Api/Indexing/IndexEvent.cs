namespace Bfs.Iop.IndexSearch.Api.Indexing;

/// <summary>Which of the two indexes an event applies to.</summary>
public enum IndexTarget
{
    /// <summary>The catalog index: datasets, data services, public services, concepts, mapping tables.</summary>
    Catalog = 0,

    /// <summary>The code-list-entry index.</summary>
    CodeListEntry = 1,
}

/// <summary>
/// One unit of work for the index writer.
/// <para>
/// The sender supplies the resource itself (<see cref="Payload"/>), not just its id — the message is
/// "ResourceId + JSON". IOP Core already holds the fully built model at the point where it triggers,
/// so it simply forwards what it has. That matters for more than convenience: re-reading the row
/// here instead would run without an HTTP user, and the Core read paths apply the caller's
/// read-authorization filter, so every non-public resource would silently never be indexed.
/// </para>
/// <para>
/// A null <see cref="Payload"/> means "remove this document" — the row is gone, so there is nothing
/// to send.
/// </para>
/// </summary>
/// <param name="Target">Which index to write to.</param>
/// <param name="Id">The resource id, used as the document id and for de-indexing.</param>
/// <param name="Payload">The resource model to index, or null to de-index.</param>
public sealed record IndexEvent(IndexTarget Target, Guid Id, object? Payload);
