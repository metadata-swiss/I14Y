using System.Globalization;
using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort;

/// <summary>
/// Compares nodes by the numeric value they carry through one element - <c>sh:order</c> being the case
/// this exists for, but the element is whatever the caller passes in.
/// </summary>
internal sealed class ElementValueComparer : IComparer<INode>
{
    private readonly Uri _element;
    private readonly Dictionary<INode, decimal> _values = [];

    public ElementValueComparer(Uri element)
    {
        _element = element ?? throw new ArgumentNullException(nameof(element));
    }

    /// <summary>
    /// Reads the element values in one pass. The values belong to the graph being written rather than
    /// to the comparer, so this runs before each sort.
    /// </summary>
    public void ReadFrom(IGraph graph)
    {
        ArgumentNullException.ThrowIfNull(graph, nameof(graph));

        _values.Clear();

        foreach (var triple in graph.GetTriplesWithPredicate(graph.CreateUriNode(_element)))
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
