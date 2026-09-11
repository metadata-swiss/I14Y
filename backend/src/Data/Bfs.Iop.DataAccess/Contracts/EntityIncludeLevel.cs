namespace Bfs.Iop.DataAccess.Contracts;

/// <summary>
/// How much of an entity's related data a read should load.
/// </summary>
/// <remarks>
/// Callers asking for <see cref="Minimal"/> get a model whose collection properties are empty
/// because they were not queried, not because the entity has none. Only ask for it when the few
/// scalar properties you need are known to be covered.
/// </remarks>
public enum EntityIncludeLevel
{
    /// <summary>Scalar properties and the publisher only. Cheap: no collection joins.</summary>
    Minimal = 1,

    /// <summary>Every related collection. Complete, but a single query with many joins.</summary>
    All = 2
}
