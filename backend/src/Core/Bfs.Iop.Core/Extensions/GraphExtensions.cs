using Bfs.Iop.Core.Extensions;
using Bfs.Iop.DataAccess.Abstractions;
using VDS.RDF;

namespace Bfs.Iop.Core.Extensions;

internal static class GraphExtensions
{
    public static void Assert(this IGraph graph, INode subj, string pred, INode obj)
        => graph.Assert(subj, graph.CreateUriNode(pred), obj);

    /// <summary>
    /// Transform obj to uri node
    /// </summary>
    public static void AssertUri(this IGraph graph, INode subj, INode pred, string obj)
        => graph.Assert(subj, pred, graph.CreateUriNode(obj));

    public static void Assert(this IGraph graph, INode subj, INode pred, MultiLanguageModel obj)
    {
        foreach (var x in obj.ToDictionary())
        {
            graph.Assert(subj, pred, graph.CreateLiteralNode(x.Value, x.Key));
        }
    }

    public static void Assert(this IGraph graph, INode subj, string pred, IEnumerable<MultiLanguageModel> obj)
    {
        foreach (var m in obj)
        {
            graph.Assert(subj, graph.CreateUriNode(pred), m);
        }
    }

    public static void Assert(this IGraph graph, INode subj, string pred, MultiLanguageModel obj)
        => graph.Assert(subj, graph.CreateUriNode(pred), obj);

    /// <summary>
    /// Transform pred to uri node and obj to literal node
    /// </summary>
    public static void AssertLiteral(this IGraph graph, INode subj, string pred, string obj)
        => graph.Assert(subj, graph.CreateUriNode(pred), graph.CreateLiteralNode(obj));

    /// <summary>
    /// Transform pred to uri node and obj to date literal node if obj is not null
    /// </summary>
    public static void Assert(this IGraph graph, INode subj, string pred, DateTimeOffset? obj)
    {
        if (obj.HasValue)
        {
            graph.Assert(subj, graph.CreateUriNode(pred), graph.CreateDateLiteral(obj.Value));
        }
    }

    public static ILiteralNode CreateDateLiteral(this IGraph graph, DateTimeOffset dt)
        => graph.CreateLiteralNode(dt.ToString("yyyy-MM-dd"), new Uri("http://www.w3.org/2001/XMLSchema#date"));
}
