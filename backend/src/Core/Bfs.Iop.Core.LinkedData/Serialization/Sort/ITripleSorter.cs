using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort;

/// <summary>
/// Sorts the triples a writer is about to emit. This is the seam that replaces
/// <c>WriterHelper.SortTriplesBySubjectPredicate</c> in <c>SortTurtleWriter</c>.
/// </summary>
internal interface ITripleSorter
{
    /// <summary>
    /// Sorts <paramref name="triples"/> in place. The graph comes along so that an implementation can
    /// index it once rather than query it on every comparison.
    /// </summary>
    void Sort(List<Triple> triples, IGraph graph);
}
