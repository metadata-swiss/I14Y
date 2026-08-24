using System.Globalization;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Serialization.Sort;
using Bfs.Iop.Core.LinkedData.Serialization.Writers;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.LinkedData.UnitTests.Serialization;

[TestFixture(TestOf = typeof(SortRdfXmlWriter))]
internal sealed class SortRdfXmlWriterTests
{
    private const string Shacl = "http://www.w3.org/ns/shacl#";
    private const string Example = "http://example.org/";
    private const string Rdf = "http://www.w3.org/1999/02/22-rdf-syntax-ns#";
    private const string Xsd = "http://www.w3.org/2001/XMLSchema#";

    /// <summary>
    /// Names whose alphabetical order is the exact reverse of their <c>sh:order</c>, asserted into the
    /// graph in alphabetical order. A writer that sorts by IRI fails, and so does one that just keeps
    /// the order the triples went in.
    /// </summary>
    private static readonly string[] NamesInShaclOrder = ["zebra", "yak", "walrus", "tiger", "shark", "rabbit"];

    [Test]
    public void Given_property_shapes_When_writing_with_shacl_sorting_Then_the_class_leads_and_its_properties_follow_sh_order()
    {
        var xml = Write(CreateShapeGraph(), Sorting());

        SubjectsInOutputOrder(xml).Should().Equal(["MyShape", .. NamesInShaclOrder]);
    }

    [Test]
    public void Given_two_classes_When_writing_Then_each_class_is_followed_by_its_own_properties()
    {
        var graph = NewGraph();

        DeclareClass(graph, "beta", [("bProp0", 0), ("bProp1", 1)]);
        DeclareClass(graph, "alpha", [("aProp0", 0), ("aProp1", 1)]);

        var xml = Write(graph, Sorting());

        SubjectsInOutputOrder(xml).Should().Equal("alpha", "aProp0", "aProp1", "beta", "bProp0", "bProp1");
    }

    [Test]
    public void Given_a_node_shape_listing_its_properties_When_writing_Then_the_property_references_follow_sh_order()
    {
        var xml = Write(CreateShapeGraph(), Sorting());

        PropertyReferencesOf(xml).Should().Equal(NamesInShaclOrder);
    }

    [Test]
    public void Given_the_same_triples_asserted_in_a_different_order_When_writing_Then_the_output_is_identical()
    {
        var fromAlphabetical = Write(CreateShapeGraph(), Sorting());
        var fromReversed = Write(CreateShapeGraph(assertInReverse: true), Sorting());

        fromReversed.Should().Be(fromAlphabetical);
    }

    [Test]
    public void Given_a_shape_graph_When_writing_and_reparsing_Then_the_graph_is_unchanged()
    {
        var graph = CreateShapeGraph();

        var reparsed = Reparse(Write(graph, Sorting()));

        reparsed.Triples.Count.Should().Be(graph.Triples.Count);
        reparsed.Equals(graph).Should().BeTrue();
    }

    [Test]
    public void Given_an_sh_order_that_is_not_a_number_When_writing_Then_the_node_is_treated_as_unranked_and_nothing_is_thrown()
    {
        var graph = CreateGraphWithOrders(("dirty", "not-a-number"), ("clean", "0"));

        var write = () => Write(graph, Sorting());

        write.Should().NotThrow();
        SubjectsInOutputOrder(write()).Should().Equal("MyShape", "clean", "dirty");
    }

    [Test]
    public void Given_a_collection_When_writing_Then_it_is_written_once_and_survives_a_round_trip()
    {
        // The collection triples are marked as written inside dotNetRDF's own TriplesDone, the set whose
        // Add is closed to us. Written twice, or dropped, the reparsed graph would differ.
        var graph = NewGraph();
        var list = graph.AssertList<INode>(
            [graph.CreateLiteralNode("a"), graph.CreateLiteralNode("b")],
            _ => graph.CreateBlankNode());

        graph.Assert(graph.CreateUriNode("ex:s"), graph.CreateUriNode("sh:in"), list);

        var reparsed = Reparse(Write(graph, Sorting()));

        reparsed.Triples.Count.Should().Be(graph.Triples.Count);
        reparsed.Equals(graph).Should().BeTrue();
    }
    private static Graph CreateShapeGraph(bool assertInReverse = false)
    {
        var graph = NewGraph();

        var shape = graph.CreateUriNode("ex:MyShape");

        graph.Assert(shape, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("sh:NodeShape"));

        var ranked = NamesInShaclOrder.Select((name, rank) => (name, rank));

        foreach (var (name, rank) in assertInReverse ? ranked : ranked.Reverse())
        {
            var property = graph.CreateUriNode($"ex:{name}");

            graph.Assert(shape, graph.CreateUriNode("sh:property"), property);
            graph.Assert(property, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("sh:PropertyShape"));
            graph.Assert(property, graph.CreateUriNode("sh:path"), graph.CreateUriNode($"ex:path_{name}"));
            graph.Assert(property, graph.CreateUriNode("sh:name"), graph.CreateLiteralNode($"Nom {name}", "fr"));
            graph.Assert(property, graph.CreateUriNode("sh:name"), graph.CreateLiteralNode($"Name {name}", "en"));
            graph.Assert(
                property,
                graph.CreateUriNode("sh:order"),
                graph.CreateLiteralNode(rank.ToString(CultureInfo.InvariantCulture), new Uri($"{Xsd}integer")));
        }

        return graph;
    }

    private static void DeclareClass(Graph graph, string className, (string Name, int Order)[] properties)
    {
        var owner = graph.CreateUriNode($"ex:{className}");

        graph.Assert(owner, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("sh:NodeShape"));

        foreach (var (name, order) in properties.Reverse())
        {
            var property = graph.CreateUriNode($"ex:{name}");

            graph.Assert(owner, graph.CreateUriNode("sh:property"), property);
            graph.Assert(property, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("sh:PropertyShape"));
            graph.Assert(
                property,
                graph.CreateUriNode("sh:order"),
                graph.CreateLiteralNode(order.ToString(CultureInfo.InvariantCulture), new Uri($"{Xsd}integer")));
        }
    }

    /// <summary>
    /// The properties hang off a class, because only the nodes that belong to one are sorted; a node
    /// that belongs to none is written in the order the graph holds it.
    /// </summary>
    private static Graph CreateGraphWithOrders(params (string Name, string Order)[] properties)
    {
        var graph = NewGraph();

        var owner = graph.CreateUriNode("ex:MyShape");

        foreach (var (name, order) in properties)
        {
            var property = graph.CreateUriNode($"ex:{name}");

            graph.Assert(owner, graph.CreateUriNode("sh:property"), property);
            graph.Assert(property, graph.CreateUriNode("sh:order"), graph.CreateLiteralNode(order, new Uri($"{Xsd}decimal")));
        }

        return graph;
    }

    private static Graph NewGraph()
    {
        var graph = new Graph();

        graph.NamespaceMap.AddNamespace("sh", new Uri(Shacl));
        graph.NamespaceMap.AddNamespace("ex", new Uri(Example));
        graph.NamespaceMap.AddNamespace("rdf", new Uri(Rdf));
        graph.NamespaceMap.AddNamespace("xsd", new Uri(Xsd));

        return graph;
    }

    private static ITripleSorter Sorting() =>
        new BlockTripleSorter(new ElementValueComparer(new Uri($"{Shacl}order")));

    private static string Write(IGraph graph, ITripleSorter sorting)
    {
        var output = new System.IO.StringWriter();

        new SortRdfXmlWriter(sorting).Save(graph, output);

        return output.ToString();
    }

    private static Graph Reparse(string xml)
    {
        var graph = new Graph();

        new RdfXmlParser().Load(graph, new System.IO.StringReader(xml));

        return graph;
    }

    /// <summary>
    /// Each subject opens an element carrying rdf:about; the entity form appears when a DTD is written.
    /// </summary>
    private static IReadOnlyList<string> SubjectsInOutputOrder(string xml) =>
        [.. Regex.Matches(xml, @"rdf:about=""(?:&\w+;|http://example\.org/)(\w+)""").Select(x => x.Groups[1].Value)];

    private static IReadOnlyList<string> PropertyReferencesOf(string xml) =>
        [.. Regex.Matches(xml, @"<sh:property rdf:resource=""(?:&\w+;|http://example\.org/)(\w+)""")
            .Select(x => x.Groups[1].Value)];
}
