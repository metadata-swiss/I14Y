using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Extensions;

internal static class DatasetStructureImportTransformerExtensions
{
    internal static void EnqueueIfNotVisited(
        this Queue<INode> queue,
        INode node,
        ISet<INode> visitedNodes)
    {
        if (visitedNodes.Add(node))
        {
            queue.Enqueue(node);
        }
    }

    internal static void AddTriplesWithSubject(
        this ISet<Triple> reachableTriples,
        Graph graph,
        INode subject)
    {
        foreach (var triple in graph.GetTriplesWithSubject(subject).ToList())
        {
            reachableTriples.Add(triple);
        }
    }
}