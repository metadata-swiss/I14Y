using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.LinkedData.Helpers;
using Bfs.Iop.DataAccess.Abstractions;
using VDS.RDF;
using VDS.RDF.Parsing;
using VDS.RDF.Update;

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

    [TestCase("line1\nline2")]
    [TestCase("say \"hi\" \\o/")]
    public void Given_description_with_special_characters_When_updating_property_Then_it_is_stored_unchanged(string description)
    {
        var datasetId = Guid.NewGuid();
        var store = CreateStructureStore(datasetId);
        var property = new SchemaProperty { Path = PropertyUri, Description = new MultiLanguageModel { En = description } };

        new LeviathanUpdateProcessor(store).ProcessCommandSet(
            UpdateParser.ParseFromString(ShaclSparqlQueryHelper.UpdatePropertyQuery(property, datasetId, ClassUri)));

        GetStoredLiterals(store, "http://www.w3.org/ns/shacl#description").Should().Equal(description);
    }

    [TestCase("line1\nline2")]
    [TestCase("say \"hi\" \\o/")]
    public void Given_description_with_special_characters_When_updating_class_Then_it_is_stored_unchanged(string description)
    {
        var datasetId = Guid.NewGuid();
        var store = CreateStructureStore(datasetId);
        var schemaClass = new SchemaClass { UriComplete = ClassUri, Description = new MultiLanguageModel { En = description } };

        new LeviathanUpdateProcessor(store).ProcessCommandSet(
            UpdateParser.ParseFromString(ShaclSparqlQueryHelper.UpdateClassQuery(schemaClass, datasetId)));

        GetStoredLiterals(store, "http://purl.org/dc/terms/description").Should().Equal(description);
    }

    private static readonly Uri ClassUri = new("https://example.org/structure/class-a");
    private static readonly Uri PropertyUri = new("https://example.org/structure/class-a/property-a");

    private static TripleStore CreateStructureStore(Guid datasetId)
    {
        var graph = new Graph(new UriNode(new Uri("urn:x-arq:DefaultGraph")));
        graph.LoadFromString($"""
            @prefix sh: <http://www.w3.org/ns/shacl#> .
            @prefix schema: <http://schema.org/#> .
            <{ClassUri}> schema:identifier "{datasetId}" ;
                a sh:NodeShape ;
                sh:property <{PropertyUri}> .
            <{PropertyUri}> a sh:PropertyShape ;
                sh:path <{PropertyUri}> .
            """);
        var store = new TripleStore();
        store.Add(graph);
        return store;
    }

    private static List<string> GetStoredLiterals(TripleStore store, string predicate) =>
        store.Triples
            .Where(t => t.Predicate is IUriNode p && p.Uri.AbsoluteUri == predicate)
            .Select(t => t.Object).OfType<ILiteralNode>().Select(n => n.Value).ToList();

    [Test]
    public void Given_duplicate_and_fragment_only_conformsTo_When_building_update_property_query_Then_each_distinct_uri_is_inserted_once()
    {
        var conceptA = new Uri("https://example.org/vocabulary#ConceptA");
        var conceptB = new Uri("https://example.org/vocabulary#ConceptB");

        var update = BuildUpdatePropertyQuery([conceptA, conceptB, conceptA]);

        UpdateParser.Invoking(p => p.ParseFromString(update)).Should().NotThrow();
        update.Should().Contain("?prop dcterms:conformsTo ?oldValue .");
        update.Should().Contain($"?prop dcterms:conformsTo <{conceptA}> .", Exactly.Once());
        update.Should().Contain($"?prop dcterms:conformsTo <{conceptB}> .", Exactly.Once());
    }

    [Test]
    public void Given_no_conformsTo_When_building_update_property_query_Then_existing_are_deleted_and_none_inserted()
    {
        var update = BuildUpdatePropertyQuery([]);

        UpdateParser.Invoking(p => p.ParseFromString(update)).Should().NotThrow();
        update.Should().Contain("?prop dcterms:conformsTo ?oldValue .");
        update.Should().NotContain("?prop dcterms:conformsTo <");
    }

    [Test]
    public void Given_blank_property_shapes_sharing_path_When_converting_graph_Then_each_keeps_its_own_conformsTo()
    {
        const string turtle = """
            @prefix sh: <http://www.w3.org/ns/shacl#> .
            @prefix dcterms: <http://purl.org/dc/terms/> .
            <https://example.org/class-a> a sh:NodeShape ;
                sh:property [
                    sh:path <https://example.org/prop-a> ;
                    sh:order 1 ;
                    dcterms:conformsTo <https://example.org/concept/b>, <https://example.org/concept/a>
                ] , [
                    sh:path <https://example.org/prop-a> ;
                    sh:order 2
                ] .
            """;
        var graph = new VDS.RDF.Graph();
        new TurtleParser().Load(graph, new StringReader(turtle));

        var schemaGraph = DatasetModelProcessHelper.ConvertGraphToSchemaGraph(graph, "test");

        var properties = schemaGraph.Classes.Should().ContainSingle().Subject.Properties;
        properties.Should().HaveCount(2);
        properties.Single(p => p.Order == 1).ConformsTo.Should().Equal(
            new Uri("https://example.org/concept/a"),
            new Uri("https://example.org/concept/b"));
        properties.Single(p => p.Order == 2).ConformsTo.Should().BeEmpty();
    }

    private static string BuildUpdatePropertyQuery(List<Uri> conformsToUris) =>
        ShaclSparqlQueryHelper.UpdatePropertyQuery(
            new SchemaProperty { Path = new Uri("https://example.org/prop-a"), ConformsTo = conformsToUris },
            Guid.Parse("18954b84-d65e-45eb-a6be-b73528f8a7c3"),
            new Uri("https://example.org/class-a"));
}
