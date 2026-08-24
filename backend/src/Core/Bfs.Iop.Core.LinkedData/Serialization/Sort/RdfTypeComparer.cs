using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort
{
    /// <summary>
    /// rdf:type leads a block, so that it reads as 'ex:zebra a sh:PropertyShape ; ...'.
    /// </summary>
    internal sealed class RdfTypeComparer : IComparer<INode>
    {
        private static readonly Uri RdfType = new(RdfSpecsHelper.RdfType);
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
