using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort;

/// <summary>
/// Sorts triples so that the blocks a writer produces - one per subject - come out in the order the
/// comparer gives.
/// </summary>
internal sealed class BlockTripleSorter : ITripleSort
{
    private static readonly Uri RdfType = new(RdfSpecsHelper.RdfType);

    /// <summary>
    /// Orders the blocks against each other, and the objects inside one predicate.
    /// </summary>
    /// <remarks>
    /// A comparer holds the values it read out of one graph, so an instance must not be shared between
    /// concurrent exports. Build one per serialization.
    /// </remarks>
    public ElementValueComparer NodeComparer { get; init; }


    public BlockTripleSorter(ElementValueComparer nodeComparer)
    {
        NodeComparer = nodeComparer;
    }
    public void Sort(List<Triple> triples, IGraph graph)
    {
        ArgumentNullException.ThrowIfNull(triples, nameof(triples));
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        NodeComparer.ReadFrom(graph);

        var sorted = triples
            .GroupBy(x => x.Subject)
            .OrderBy(block => block.Key, NodeComparer)
            .SelectMany(block => block
                .OrderBy(x => x.Predicate, PredicateComparer.Instance)
                .ThenBy(x => x.Object, NodeComparer))
            .ToList();

        triples.Clear();
        triples.AddRange(sorted);
    }

    /// <summary>
    /// rdf:type leads a block, so that it reads as 'ex:zebra a sh:PropertyShape ; ...'.
    /// </summary>
    private sealed class PredicateComparer : IComparer<INode>
    {
        public static readonly PredicateComparer Instance = new();

        public int Compare(INode? x, INode? y)
        {
            var xIsType = IsRdfType(x);
            var yIsType = IsRdfType(y);

            return xIsType == yIsType
                ? string.CompareOrdinal(x?.ToString(), y?.ToString())
                : xIsType ? -1 : 1;
        }

        private static bool IsRdfType(INode? node) =>
            node is IUriNode uriNode && EqualityHelper.AreUrisEqual(uriNode.Uri, RdfType);
    }
}
