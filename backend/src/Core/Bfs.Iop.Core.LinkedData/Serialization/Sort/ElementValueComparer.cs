using System.Globalization;
using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort;

/// <summary>
/// Compares nodes by the numeric value they carry through one element - <c>sh:order</c> being the case
/// this exists for, but the element is whatever the caller passes in.
/// </summary>
internal sealed class ElementValueComparer : IComparer<INode>
{
    private readonly Dictionary<INode, decimal> _values = [];

    /// <summary>
    /// Initializes a comparer that reads the values of <paramref name="element"/> in <paramref name="graph"/>.
    /// </summary>
    public ElementValueComparer(IGraph graph, Uri element)
    {
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));
        ArgumentNullException.ThrowIfNull(element, nameof(element));
        // Reads the element values in one pass
        foreach (var triple in graph.GetTriplesWithPredicate(graph.CreateUriNode(element)))
        {
            // A value that cannot be read counts as absent: an export must not fail on dirty data.
            if (triple.Object is ILiteralNode literal &&
                decimal.TryParse(literal.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var value))
            {
                _values[triple.Subject] = value;
            }
        }
    }

    public int Compare(INode? x, INode? y)
    {
        if (ReferenceEquals(x, y))
        {
            return 0;
        }

        if (x is null || y is null)
        {
            return x is null ? -1 : 1;
        }

        var xHasValue = _values.TryGetValue(x, out var xValue);
        var yHasValue = _values.TryGetValue(y, out var yValue);

        // A shape graph always holds nodes without a value - the node shape itself, the sh:in list
        // cells - and they go after the ones that carry it.
        if (xHasValue != yHasValue)
        {
            return xHasValue ? -1 : 1;
        }

        return xHasValue && xValue != yValue
            ? xValue.CompareTo(yValue)
            // Two nodes can share a value, or share the absence of one, and the file still has to be
            // reproducible from one export to the next.
            : string.CompareOrdinal(x.ToString(), y.ToString());
    }
}
