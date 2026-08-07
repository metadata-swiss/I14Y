using AwesomeAssertions;
using Bfs.Iop.Core.LinkedData.Helpers;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Writing;

namespace Bfs.Iop.Core.LinkedData.UnitTests;

[TestFixture(TestOf = typeof(ShaclSparqlQueryHelper))]
internal sealed class ShaclSparqlQueryHelperTests
{
    private static readonly SparqlQueryParser Parser = new();
    private static readonly SparqlUpdateParser UpdateParser = new();

    [Test]
    public void Given_valid_concept_iri_When_building_count_query_Then_query_is_parseable_and_contains_the_iri()
    {
        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery(
            ["https://example.org/i14y/concept/c1/version/1.0.0"]);

        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        query.Should().Contain("<https://example.org/i14y/concept/c1/version/1.0.0>");
    }

    [Test]
    public void Given_malicious_identifier_When_building_count_query_Then_query_stays_parseable_and_is_not_injected()
    {
        // A client-supplied identifier crafted to break out of the <...> IRI token and inject SPARQL.
        var malicious = "https://example.org/i14y/concept/x> } } INSERT DATA { <a> <b> <c> } #/version/1";

        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery([malicious]);

        // The query must remain syntactically valid (no break-out) ...
        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        // ... and must not contain the raw injected SPARQL keyword as executable text.
        query.Should().NotContain("INSERT DATA");
    }

    [Test]
    public void Given_non_absolute_iri_When_building_count_query_Then_it_is_skipped_and_query_is_parseable()
    {
        var query = ShaclSparqlQueryHelper.GetConceptStructureReferencesQuery(["not-an-absolute-uri"]);

        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        query.Should().NotContain("not-an-absolute-uri");
    }

    [Test]
    public void Given_dataset_and_property_uri_When_building_delete_property_update_Then_update_is_parseable_and_guarded()
    {
        var datasetId = Guid.Parse("18954b84-d65e-45eb-a6be-b73528f8a7c3");
        var propertyUri = new Uri("https://example.org/i14y/dataset/ds-1/structure/class-a/property-a");

        var update = ShaclSparqlQueryHelper.DeleteSchemaPropertyQuery(datasetId, propertyUri);

        UpdateParser.Invoking(p => p.ParseFromString(update)).Should().NotThrow();
        update.Should().Contain($"?root schema:identifier \"{datasetId}\" .");
        update.Should().Contain($"<{propertyUri}> ?p ?o .");
        update.Should().Contain($"?parent sh:property <{propertyUri}> .");
        update.Should().Contain("?parent a sh:NodeShape");
        update.Should().Contain($"<{propertyUri}> a sh:PropertyShape ;");
        update.Should().Contain($"sh:path <{propertyUri}> .");
        update.Should().Contain("?list rdf:rest* ?listNode .");
    }

    [Test]
    public void Given_dataset_and_property_uri_When_building_property_exists_query_Then_query_is_parseable_and_type_guarded()
    {
        var datasetId = Guid.Parse("76324802-cc57-47e0-b673-8673c6677151");
        var propertyUri = new Uri("https://example.org/i14y/dataset/ds-2/structure/class-b/property-b");

        var query = ShaclSparqlQueryHelper.SchemaPropertyExistsQuery(datasetId, propertyUri);

        Parser.Invoking(p => p.ParseFromString(query)).Should().NotThrow();
        query.Should().Contain("ASK {");
        query.Should().Contain($"?root schema:identifier \"{datasetId}\" ;");
        query.Should().Contain($"?nodeShape a sh:NodeShape ;");
        query.Should().Contain($"sh:property <{propertyUri}> .");
        query.Should().Contain($"<{propertyUri}> a sh:PropertyShape ;");
        query.Should().Contain($"sh:path <{propertyUri}> .");
    }

    [Test]
    public void SortPropertyShapeBlocksByShOrder_can_sort_shacl_property_triples_according_shacl_order()
    {
        var datasetId = Guid.NewGuid();
        var graph = new Graph();

        var sh = "http://www.w3.org/ns/shacl#";
        var xsd = "http://www.w3.org/2001/XMLSchema#";

        var nodeShape = graph.CreateUriNode(new Uri("https://example.org/ds/NodeShape"));
        var rdfType = graph.CreateUriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
        var shNodeShape = graph.CreateUriNode(new Uri(sh + "NodeShape"));
        var shPropertyShape = graph.CreateUriNode(new Uri(sh + "PropertyShape"));
        var shProperty = graph.CreateUriNode(new Uri(sh + "property"));
        var shOrder = graph.CreateUriNode(new Uri(sh + "order"));
        var xsdInteger = new Uri(xsd + "integer");

        graph.Assert(nodeShape, rdfType, shNodeShape);

        // Insert PropertyShapes in a non-sorted order and with sh:order values in a different order.
        var propertyOrders = new (string LocalName, int Order)[]
        {
            ("status_q", 8),
            ("component", 1),
            ("period", 0),
            ("month", 4),
            ("value", 5),
        };

        foreach (var (localName, order) in propertyOrders)
        {
            var prop = graph.CreateUriNode(new Uri($"https://example.org/ds/NodeShape/{localName}"));
            graph.Assert(nodeShape, shProperty, prop);
            graph.Assert(prop, rdfType, shPropertyShape);
            graph.Assert(prop, shOrder, graph.CreateLiteralNode(order.ToString(System.Globalization.CultureInfo.InvariantCulture), xsdInteger));
        }

        ShaclSparqlQueryHelper.CleanStructureBeforeExport(graph, datasetId);

        // Serialize to Turtle and sort PropertyShape blocks by sh:order.
        using var writer = new System.IO.StringWriter();
        new CompressingTurtleWriter().Save(graph, writer);
        var ttl = ShaclSparqlQueryHelper.SortPropertyShapeBlocksByShOrder(writer.ToString(), graph);
        TestContext.Out.WriteLine(ttl);

        var expectedOrder = new[] { "period", "component", "month", "value", "status_q" };

        // Position of each PropertyShape subject block (line beginning with the
        // property IRI and containing "a <...#PropertyShape>").
        int FindBlockIndex(string localName)
        {
            var needle = $"<https://example.org/ds/NodeShape/{localName}> a <http://www.w3.org/ns/shacl#PropertyShape>";
            return ttl.IndexOf(needle, StringComparison.Ordinal);
        }

        var subjectBlockIndices = expectedOrder.Select(FindBlockIndex).ToList();

        subjectBlockIndices.Should().OnlyContain(i => i >= 0, "each PropertyShape subject block should be present in the Turtle output");
        subjectBlockIndices.Should().BeInAscendingOrder("PropertyShape subject blocks should be ordered by sh:order in the Turtle output");
    }

    [Test]
    public void CleanStructureBeforeExport_can_sort_shape_blocks_according_shacl_order()
    {
        var datasetId = Guid.NewGuid();
        var graph = new Graph();

        var sh = "http://www.w3.org/ns/shacl#";
        var xsd = "http://www.w3.org/2001/XMLSchema#";

        var rdfType = graph.CreateUriNode(new Uri("http://www.w3.org/1999/02/22-rdf-syntax-ns#type"));
        var shNodeShape = graph.CreateUriNode(new Uri(sh + "NodeShape"));
        var shPropertyShape = graph.CreateUriNode(new Uri(sh + "PropertyShape"));
        var shProperty = graph.CreateUriNode(new Uri(sh + "property"));
        var shOrder = graph.CreateUriNode(new Uri(sh + "order"));
        var xsdInteger = new Uri(xsd + "integer");

        void AddNodeShape(string nodeShapeLocal, params (string LocalName, int Order)[] props)
        {
            var nodeShape = graph.CreateUriNode(new Uri($"https://example.org/ds/{nodeShapeLocal}"));
            graph.Assert(nodeShape, rdfType, shNodeShape);

            foreach (var (localName, order) in props)
            {
                var prop = graph.CreateUriNode(new Uri($"https://example.org/ds/{nodeShapeLocal}/{localName}"));
                graph.Assert(nodeShape, shProperty, prop);
                graph.Assert(prop, rdfType, shPropertyShape);
                graph.Assert(prop, shOrder, graph.CreateLiteralNode(order.ToString(System.Globalization.CultureInfo.InvariantCulture), xsdInteger));
            }
        }

        // Two NodeShapes with intentionally interleaving alphabetical property names.
        AddNodeShape("AShape", ("z_alpha", 2), ("a_alpha", 0), ("m_alpha", 1));
        AddNodeShape("BShape", ("z_beta", 2), ("a_beta", 0), ("m_beta", 1));

        ShaclSparqlQueryHelper.CleanStructureBeforeExport(graph, datasetId);

        using var writer = new System.IO.StringWriter();
        new CompressingTurtleWriter().Save(graph, writer);
        var ttl = ShaclSparqlQueryHelper.sortPropertyShapeBlocksByShOrder(writer.ToString(), graph);

        TestContext.Out.WriteLine(ttl);

        int FindBlockIndex(string nodeShapeLocal, string localName)
        {
            var needle = $"<https://example.org/ds/{nodeShapeLocal}/{localName}> a <http://www.w3.org/ns/shacl#PropertyShape>";
            return ttl.IndexOf(needle, StringComparison.Ordinal);
        }

        // Within each NodeShape group, order must follow sh:order 0,1,2.
        var aIndices = new[] { "a_alpha", "m_alpha", "z_alpha" }.Select(n => FindBlockIndex("AShape", n)).ToList();
        var bIndices = new[] { "a_beta", "m_beta", "z_beta" }.Select(n => FindBlockIndex("BShape", n)).ToList();

        aIndices.Should().OnlyContain(i => i >= 0);
        bIndices.Should().OnlyContain(i => i >= 0);
        aIndices.Should().BeInAscendingOrder("AShape's PropertyShape blocks should be ordered by sh:order");
        bIndices.Should().BeInAscendingOrder("BShape's PropertyShape blocks should be ordered by sh:order");
    }
}
