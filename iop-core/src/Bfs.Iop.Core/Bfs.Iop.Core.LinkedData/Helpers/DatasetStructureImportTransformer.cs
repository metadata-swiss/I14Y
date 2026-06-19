using Bfs.Iop.Core.LinkedData.Extensions;
using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Helpers;

internal static class DatasetStructureImportTransformer
{
    private static readonly Uri _dctermsConformsToUri = new("http://purl.org/dc/terms/conformsTo");
    private static readonly Uri _dctermsHasPartUri = new("http://purl.org/dc/terms/hasPart");
    private static readonly Uri _owlOntologyUri = new("http://www.w3.org/2002/07/owl#Ontology");
    private static readonly Uri _rdfTypeUri = new("http://www.w3.org/1999/02/22-rdf-syntax-ns#type");
    private static readonly Uri _shNodeShapeUri = new("http://www.w3.org/ns/shacl#NodeShape");
    private static readonly Uri _shPathUri = new("http://www.w3.org/ns/shacl#path");
    private static readonly Uri _shPropertyShapeUri = new("http://www.w3.org/ns/shacl#PropertyShape");
    private static readonly Uri _shPropertyUri = new("http://www.w3.org/ns/shacl#property");
    private static readonly Uri _schemaIdentifier = new("http://schema.org/#identifier");

    internal static async Task<Graph> TransformImportedGraph(
        Graph graph,
        Guid datasetId,
        string datasetIdentifier,
        string baseIriUrl,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentException.ThrowIfNullOrWhiteSpace(datasetIdentifier);

        // 1. Normalize SHACL shape IRIs:
        //    Rebuild NodeShape and PropertyShape IRIs so they follow the dataset structure pattern (NodeShapeIriPattern + PropertyShapeIriPattern)
        ReplaceShapeIris(graph, datasetIdentifier, baseIriUrl);

        // 2. Attach the structure root:
        //    Link all NodeShapes to the dataset structure root via dcterms:hasPart
        //    (creates the entry point used later by SPARQL queries)
        AttachStructureRoot(graph, datasetId, datasetIdentifier, baseIriUrl);

        // 3. Attach ontology resources:
        //    Link ontology nodes (e.g. SPHN ontologies) to the structure root
        //    (ensures they are included in traversal and not lost)
        AttachOntologyToStructure(graph, datasetIdentifier, baseIriUrl);

        // 4. Clean orphan triples:
        //    Remove any triples that are not reachable from the structure root
        //    after transformation (e.g. leftover blank nodes or detached subgraphs)
        //    (prevents inconsistencies and avoids leaking unused data)
        PruneOrphanedStructureTriples(graph, datasetIdentifier, baseIriUrl);

        return graph;
    }

    private static void ReplaceShapeIris(Graph graph, string datasetIdentifier, string baseIriUrl)
    {
        var NodeShapeIriPattern = ShaclSparqlQueryHelper.GetNodeShapeIriPattern(datasetIdentifier, baseIriUrl);
        var PropertyShapeIriPattern = ShaclSparqlQueryHelper.GetPropertyShapeIriPattern(datasetIdentifier, baseIriUrl);

        var rdfTypeNode = graph.CreateUriNode(_rdfTypeUri);
        var shNodeShapeNode = graph.CreateUriNode(_shNodeShapeUri);
        var shPropertyNode = graph.CreateUriNode(_shPropertyUri);
        var shPathNode = graph.CreateUriNode(_shPathUri);

        var nodeShapeMapping = new Dictionary<IUriNode, IUriNode>();
        var propertyShapeMapping = new Dictionary<IUriNode, IUriNode>();

        // 1) Build NodeShape mapping
        foreach (var triple in graph.GetTriplesWithPredicateObject(rdfTypeNode, shNodeShapeNode).ToList())
        {
            if (triple.Subject is not IUriNode nodeShape)
            {
                continue;
            }

            var nodeShapeName = ExtractLocalName(nodeShape.Uri.AbsoluteUri);
            var newUri = new Uri(string.Format(NodeShapeIriPattern, datasetIdentifier, nodeShapeName));

            if (newUri != nodeShape.Uri)
            {
                nodeShapeMapping[nodeShape] = graph.CreateUriNode(newUri);
            }
        }

        // 2) Apply NodeShape replacements everywhere
        ReplaceNodes(graph, nodeShapeMapping);

        // 3) Build PropertyShape mapping based on updated parent NodeShape IRIs
        foreach (var triple in graph.GetTriplesWithPredicate(shPropertyNode).ToList())
        {
            if (triple.Subject is not IUriNode parentNodeShape)
            {
                continue;
            }

            // keep blank nodes as blank nodes
            if (triple.Object is not IUriNode propertyShape)
            {
                continue;
            }

            var nodeShapeName = ExtractLocalName(parentNodeShape.Uri.AbsoluteUri);
            var propertyShapeName = ExtractLocalName(propertyShape.Uri.AbsoluteUri);

            var newUri = new Uri(string.Format(
                PropertyShapeIriPattern,
                datasetIdentifier,
                nodeShapeName,
                propertyShapeName));

            if (newUri != propertyShape.Uri)
            {
                propertyShapeMapping[propertyShape] = graph.CreateUriNode(newUri);
            }
        }

        // 4) Apply PropertyShape replacements everywhere
        ReplaceNodes(graph, propertyShapeMapping);

        // 5) Normalize sh:path only on PropertyShapes that were renamed.
        //    If a renamed PropertyShape still has a URI-valued sh:path pointing to the old imported IRI, replace it with the new normalized PropertyShape IRI.
        //    Do not touch other sh:path values.
        var pathToRemove = new List<Triple>();
        var pathToAdd = new List<Triple>();

        foreach (var newPropertyShape in propertyShapeMapping.Values)
        {
            foreach (var triple in graph.GetTriplesWithSubjectPredicate(newPropertyShape, shPathNode).ToList())
            {
                if (triple.Object is not IUriNode currentPath)
                {
                    continue;
                }

                if (currentPath.Uri == newPropertyShape.Uri)
                {
                    continue;
                }

                pathToRemove.Add(triple);
                pathToAdd.Add(new Triple(newPropertyShape, shPathNode, newPropertyShape));
            }
        }

        if (pathToRemove.Count > 0)
        {
            graph.Retract(pathToRemove);
            graph.Assert(pathToAdd);
        }
    }

    private static void ReplaceNodes(Graph graph, IReadOnlyDictionary<IUriNode, IUriNode> replacements)
    {
        if (replacements.Count == 0)
        {
            return;
        }

        var toRemove = new List<Triple>();
        var toAdd = new List<Triple>();

        foreach (var triple in graph.Triples.ToList())
        {
            var subject = triple.Subject;
            var obj = triple.Object;
            var changed = false;

            if (subject is IUriNode subjectUri &&
                replacements.TryGetValue(subjectUri, out var newSubject))
            {
                subject = newSubject;
                changed = true;
            }

            if (obj is IUriNode objectUri &&
                replacements.TryGetValue(objectUri, out var newObject))
            {
                obj = newObject;
                changed = true;
            }

            if (!changed)
            {
                continue;
            }

            toRemove.Add(triple);
            toAdd.Add(new Triple(subject, triple.Predicate, obj));
        }

        if (toRemove.Count > 0)
        {
            graph.Retract(toRemove);
        }

        if (toAdd.Count > 0)
        {
            graph.Assert(toAdd);
        }
    }

    private static void AttachStructureRoot(Graph graph, Guid datasetId, string datasetIdentifier, string baseIriUrl)
    {
        var structureRoot = graph.CreateUriNode(
            new Uri(ShaclSparqlQueryHelper.GetStructureRootUri(datasetIdentifier, baseIriUrl)));

        var rdfTypeNode = graph.CreateUriNode(_rdfTypeUri);
        var shNodeShapeNode = graph.CreateUriNode(_shNodeShapeUri);
        var hasPartNode = graph.CreateUriNode(_dctermsHasPartUri);

        var schemaIdentifierNode = graph.CreateUriNode(_schemaIdentifier);

        var triplesToAdd = new List<Triple>();

        foreach (var triple in graph.GetTriplesWithPredicateObject(rdfTypeNode, shNodeShapeNode).ToList())
        {
            if (triple.Subject is IUriNode nodeShape)
            {
                triplesToAdd.Add(new Triple(structureRoot, hasPartNode, nodeShape));
            }
        }

        triplesToAdd.Add(new Triple(structureRoot, schemaIdentifierNode, graph.CreateLiteralNode(datasetId.ToString())));

        graph.Assert(triplesToAdd);
        
    }

    private static void AttachOntologyToStructure(Graph graph, string datasetIdentifier,string baseIriUrl)
    {
        var structureRoot = graph.CreateUriNode(
            new Uri(ShaclSparqlQueryHelper.GetStructureRootUri(datasetIdentifier, baseIriUrl)));

        var rdfTypeNode = graph.CreateUriNode(_rdfTypeUri);
        var owlOntologyNode = graph.CreateUriNode(_owlOntologyUri);
        var hasPartNode = graph.CreateUriNode(_dctermsHasPartUri);

        var triplesToAdd = new List<Triple>();

        foreach (var triple in graph.GetTriplesWithPredicateObject(rdfTypeNode, owlOntologyNode).ToList())
        {
            if (triple.Subject is IUriNode ontologyNode)
            {
                triplesToAdd.Add(new Triple(structureRoot, hasPartNode, ontologyNode));
            }
        }

        if (triplesToAdd.Count > 0)
        {
            graph.Assert(triplesToAdd);
        }
    }

    private static void PruneOrphanedStructureTriples(Graph graph, string datasetIdentifier, string baseIriUrl)
    {
        var rdfTypeNode = graph.CreateUriNode(_rdfTypeUri);
        var shNodeShapeNode = graph.CreateUriNode(_shNodeShapeUri);
        var shPropertyNode = graph.CreateUriNode(_shPropertyUri);
        var shPropertyShapeNode = graph.CreateUriNode(_shPropertyShapeUri);
        var owlOntologyNode = graph.CreateUriNode(_owlOntologyUri);
        var structureRoot = graph.CreateUriNode(new Uri(ShaclSparqlQueryHelper.GetStructureRootUri(datasetIdentifier, baseIriUrl)));

        var reachableTriples = new HashSet<Triple>();
        var visitedNodes = new HashSet<INode>();
        var queue = new Queue<INode>();

        queue.EnqueueIfNotVisited(structureRoot, visitedNodes);

        foreach (var triple in graph.GetTriplesWithPredicateObject(rdfTypeNode, shNodeShapeNode).ToList())
        {
            if (triple.Subject is IUriNode nodeShape)
            {
                var hasPartTriple = new Triple(structureRoot, graph.CreateUriNode(_dctermsHasPartUri), nodeShape);
                if (graph.ContainsTriple(hasPartTriple))
                {
                    reachableTriples.Add(hasPartTriple);
                    queue.EnqueueIfNotVisited(nodeShape, visitedNodes);
                }
            }
        }

        foreach (var triple in graph.GetTriplesWithPredicateObject(rdfTypeNode, owlOntologyNode).ToList())
        {
            if (triple.Subject is IUriNode ontology)
            {
                var hasPartTriple = new Triple(structureRoot, graph.CreateUriNode(_dctermsHasPartUri), ontology);
                if (graph.ContainsTriple(hasPartTriple))
                {
                    reachableTriples.Add(hasPartTriple);
                    queue.EnqueueIfNotVisited(ontology, visitedNodes);
                }
            }
        }

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            reachableTriples.AddTriplesWithSubject(graph, current);

            if (current is IUriNode currentUriNode)
            {
                var isNodeShape = graph.ContainsTriple(new Triple(currentUriNode, rdfTypeNode, shNodeShapeNode));
                var isPropertyShape = graph.ContainsTriple(new Triple(currentUriNode, rdfTypeNode, shPropertyShapeNode));

                if (isNodeShape)
                {
                    foreach (var propertyTriple in graph.GetTriplesWithSubjectPredicate(current, shPropertyNode).ToList())
                    {
                        reachableTriples.Add(propertyTriple);

                        if (propertyTriple.Object is IUriNode propertyShapeUri)
                        {
                            queue.EnqueueIfNotVisited(propertyShapeUri, visitedNodes);
                        }
                        else if (propertyTriple.Object is IBlankNode propertyShapeBlank)
                        {
                            queue.EnqueueIfNotVisited(propertyShapeBlank, visitedNodes);
                        }
                    }
                }

                if (isPropertyShape)
                {
                    TraverseNestedBlankNodes(graph, current, reachableTriples, visitedNodes, queue);
                }
            }
            else if (current is IBlankNode)
            {
                TraverseNestedBlankNodes(graph, current, reachableTriples, visitedNodes, queue);
            }
        }

        var toRemove = graph.Triples.Where(t => !reachableTriples.Contains(t)).ToList();
        if (toRemove.Count > 0)
        {
            graph.Retract(toRemove);
        }
    }

    private static void TraverseNestedBlankNodes(
        Graph graph,
        INode startNode,
        HashSet<Triple> reachableTriples,
        HashSet<INode> visitedNodes,
        Queue<INode> queue)
    {
        foreach (var triple in graph.GetTriplesWithSubject(startNode).ToList())
        {
            if (triple.Predicate is not IUriNode predicateNode)
            {
                continue;
            }

            reachableTriples.Add(triple);

            if (predicateNode.Uri == _dctermsConformsToUri)
            {
                continue;
            }

            if (triple.Object is IBlankNode blankNode)
            {
                queue.EnqueueIfNotVisited(blankNode, visitedNodes);
            }
        }
    }

    private static string ExtractLocalName(string uri)
    {
        // Extract the "local name" of a URI, i.e. the last substring after # or /
        // # takes precedence over path segments, as per RDF conventions
        var stripped = uri.TrimEnd('/');

        var relevant = stripped.Contains('#')
            ? stripped[(stripped.LastIndexOf('#') + 1)..]
            : stripped;

        var slashIndex = relevant.LastIndexOf('/');
        return slashIndex >= 0 ? relevant[(slashIndex + 1)..] : relevant;
    }

}
