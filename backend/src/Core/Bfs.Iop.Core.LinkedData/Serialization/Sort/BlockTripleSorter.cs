using VDS.RDF;

namespace Bfs.Iop.Core.LinkedData.Serialization.Sort;

/// <summary>
/// Sorts triples so that a structure reads class by class: each class first, then its own properties in
/// the order the comparer gives.
/// </summary>
internal sealed class BlockTripleSorter : ITripleSorter
{
    /// <summary>
    /// Ties a class to its properties. Grouping keys on this rather than on rdf:type, so it holds
    /// whether a class is a sh:NodeShape, an rdfs:Class, or carries no type at all.
    /// </summary>
    private static readonly Uri ShaclProperty = new("http://www.w3.org/ns/shacl#property");

    private readonly ElementValueComparer _nodeComparer;

    /// <param name="nodeComparer">Orders the properties inside a class.</param>
    public BlockTripleSorter(ElementValueComparer nodeComparer)
    {
        _nodeComparer = nodeComparer ?? throw new ArgumentNullException(nameof(nodeComparer));
    }

    public void Sort(List<Triple> triples, IGraph graph)
    {
        _nodeComparer.ReadFrom(graph);

        var classToProperty = graph.CreateUriNode(ShaclProperty);

        // Step 1: groupe triple by class.
        var groupByClass = new SortedDictionary<string, List<Triple>>(StringComparer.Ordinal);
        var tripleOutofClass = new List<Triple>();

        // Step 2: Classified per subject rather than per triple: every triple of one subject has the same class,
        foreach (var block in triples.GroupBy(x => x.Subject))
        {
            var classIri = ClassIriOf(block.Key, graph, classToProperty);
            if (classIri is null)
            {
                tripleOutofClass.AddRange(block);
                continue;
            }
            if (!groupByClass.TryGetValue(classIri, out var classTriples))
            {
                groupByClass[classIri] = classTriples = [];
            }
            classTriples.AddRange(block);
        }

        // Step 2: order inside each class; the ones that belong to none keep the graph's order.
        triples.Clear();

        foreach (var (classIri, classTriples) in groupByClass)
        {
            triples.AddRange(SortInsideClass(classTriples, classIri));
        }

        triples.AddRange(tripleOutofClass);
    }

    /// <summary>
    /// The class block first, then its properties by element value. Inside a block, rdf:type leads.
    /// </summary>
    private List<Triple> SortInsideClass(List<Triple> classTriples, string? classIri) =>
    [
        .. classTriples
            // group and sort by class, one block per class
            .GroupBy(x => x.Subject) 
            .OrderBy(block => block.Key.ToString() == classIri ? 0 : 1)
            .ThenBy(block => block.Key, _nodeComparer)
            // sort inside each block
            .SelectMany(block => block
                .OrderBy(x => x.Predicate, new RdfTypeComparer())
                .ThenBy(x => x.Object, _nodeComparer))
    ];

    /// <summary>
    /// The IRI of the class a node belongs to
    /// </summary>
    private static string? ClassIriOf(INode subject, IGraph graph, INode classToProperty)
    {
        if (graph.GetTriplesWithSubjectPredicate(subject, classToProperty).Any())
        {
            return subject.ToString();
        }

        // A property declared by two classes stays with the first, so it is written once rather than
        // pulled back and forth.
        return graph.GetTriplesWithPredicateObject(classToProperty, subject).FirstOrDefault()?.Subject.ToString();
    }
}
