using System.Globalization;
using System.Text.RegularExpressions;
using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Serialization.Sort;
using Bfs.Iop.Core.LinkedData.Serialization.Writers;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.LinkedData.UnitTests.Serialization;

[TestFixture(TestOf = typeof(SortTurtleWriter))]
internal sealed class SortTurtleWriterTests
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
    public void Given_property_shapes_When_writing_with_shacl_sorting_Then_the_subjects_follow_sh_order_and_the_node_shape_comes_last()
    {
        var turtle = Write(CreateShapeGraph(), Sorting());

        // MyShape carries no sh:order, so it lands after everything that does.
        SubjectsInOutputOrder(turtle).Should().Equal([.. NamesInShaclOrder, "MyShape"]);
    }

    [Test]
    public void Given_a_node_shape_listing_its_properties_When_writing_with_shacl_sorting_Then_the_object_list_follows_sh_order()
    {
        var turtle = Write(CreateShapeGraph(), Sorting());

        NamesIn(ObjectListOf(turtle, "sh:property")).Should().Equal(NamesInShaclOrder);
    }

    [Test]
    public void Given_the_same_triples_asserted_in_a_different_order_When_writing_Then_the_output_is_identical()
    {
        var fromAlphabetical = Write(CreateShapeGraph(), Sorting());
        var fromReversed = Write(CreateShapeGraph(assertInReverse: true), Sorting());

        fromReversed.Should().Be(fromAlphabetical);
    }

    [Test]
    public void Given_property_shapes_sharing_one_sh_order_value_When_writing_Then_they_are_still_written_in_a_reproducible_order()
    {
        var graph = CreateGraphWithOrders(("beta", "1"), ("alpha", "1"), ("gamma", "0"));

        var turtle = Write(graph, Sorting());

        // gamma is ranked ahead; alpha and beta tie on 1 and fall back to the ordinal tie-breaker.
        SubjectsInOutputOrder(turtle).Should().Equal("gamma", "alpha", "beta");
    }

    [Test]
    public void Given_an_sh_order_that_is_not_a_number_When_writing_Then_the_node_is_treated_as_unranked_and_nothing_is_thrown()
    {
        var graph = CreateGraphWithOrders(("dirty", "not-a-number"), ("clean", "0"));

        var write = () => Write(graph, Sorting());

        write.Should().NotThrow();
        SubjectsInOutputOrder(write()).Should().Equal("clean", "dirty");
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
    public void Given_a_graph_whose_subjects_are_nearly_all_distinct_When_writing_Then_compression_is_still_applied()
    {
        // Upstream this ratio switches on high speed mode, which drops to one full triple per line and
        // ignores the sort entirely. That branch is gone, so the output must stay compressed.
        var graph = NewGraph();

        for (var i = 0; i < 10; i++)
        {
            graph.Assert(graph.CreateUriNode($"ex:s{i}"), graph.CreateUriNode("sh:path"), graph.CreateUriNode($"ex:p{i}"));
        }

        var turtle = Write(graph, Sorting());

        turtle.Should().Contain("sh:path ex:p0");
        turtle.Should().NotContain($"<{Example}s0>");
    }

    [Test]
    public void Given_a_shacl_list_When_writing_Then_the_collection_syntax_of_the_original_writer_is_kept()
    {
        // The reason for copying the writer rather than rewriting it: sh:in lists stay as ( ... ).
        var graph = NewGraph();
        var list = graph.AssertList<INode>(
            [graph.CreateLiteralNode("a"), graph.CreateLiteralNode("b")],
            _ => graph.CreateBlankNode());

        graph.Assert(graph.CreateUriNode("ex:s"), graph.CreateUriNode("sh:in"), list);

        var turtle = Write(graph, Sorting());

        turtle.Should().Contain("sh:in (");
        Reparse(turtle).Equals(graph).Should().BeTrue();
    }

    [Test]
    public void Given_an_sh_order_typed_as_decimal_When_writing_and_reparsing_Then_the_value_survives_even_though_the_datatype_does_not()
    {
        // Turtle abbreviates "0"^^xsd:decimal to a bare 0, which comes back as an xsd:integer. This is
        // inherited from dotNetRDF - the unmodified CompressingTurtleWriter loses it in the same way -
        // and it is harmless here because the structure is read by value, not by datatype.
        var graph = CreateGraphWithOrders(("zebra", "0"));

        var reparsed = Reparse(Write(graph, Sorting()));

        var literal = reparsed.Triples.Single().Object.Should().BeAssignableTo<ILiteralNode>().Subject;
        literal.Value.Should().Be("0");
    }

    private static Graph CreateShapeGraph(bool assertInReverse = false)
    {
        var graph = NewGraph();

        var shape = graph.CreateUriNode("ex:MyShape");

        graph.Assert(shape, graph.CreateUriNode("rdf:type"), graph.CreateUriNode("sh:NodeShape"));

        var ranked = NamesInShaclOrder.Select((name, rank) => (name, rank));

        // Asserted alphabetically by default, which is the reverse of the expected output.
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

    private static Graph CreateGraphWithOrders(params (string Name, string Order)[] properties)
    {
        var graph = NewGraph();

        foreach (var (name, order) in properties)
        {
            graph.Assert(
                graph.CreateUriNode($"ex:{name}"),
                graph.CreateUriNode("sh:order"),
                graph.CreateLiteralNode(order, new Uri($"{Xsd}decimal")));
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

    private static ITripleSort Sorting() =>
        new BlockTripleSorter(new ElementValueComparer(new Uri($"{Shacl}order")));

    private static string Write(IGraph graph, ITripleSort sorting)
    {
        var output = new System.IO.StringWriter();

        new SortTurtleWriter(sorting).Save(graph, output);

        return output.ToString();
    }

    private static Graph Reparse(string turtle)
    {
        var graph = new Graph();

        new TurtleParser().Load(graph, new System.IO.StringReader(turtle));

        return graph;
    }

    /// <summary>
    /// Subjects start a line at column 0; the writer indents everything else under them.
    /// </summary>
    private static IReadOnlyList<string> SubjectsInOutputOrder(string turtle) =>
        [.. Regex.Matches(turtle, @"^ex:(\w+)", RegexOptions.Multiline).Select(x => x.Groups[1].Value)];

    /// <summary>
    /// The text from a predicate up to the ';' or '.' that closes its object list.
    /// </summary>
    private static string ObjectListOf(string turtle, string predicate)
    {
        var start = turtle.IndexOf(predicate, StringComparison.Ordinal);

        start.Should().BeGreaterThanOrEqualTo(0, "the output should contain {0}", predicate);

        var end = turtle.IndexOfAny([';', '.'], start);

        return turtle[start..(end < 0 ? turtle.Length : end)];
    }

    private static IReadOnlyList<string> NamesIn(string text) =>
        [.. Regex.Matches(text, @"ex:(\w+)").Select(x => x.Groups[1].Value)];
}
