using AwesomeAssertions;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Serialization.Rdf;
using Bfs.Iop.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using VDS.RDF;
using VDS.RDF.Parsing;

namespace Bfs.Iop.Core.UnitTests.Serialization.Rdf;

[TestFixture(TestOf = typeof(AgentRdfSerializer))]
internal class AgentRdfSerializerTests
{
    private const string OrgNs = "http://www.w3.org/ns/org#";
    private const string FoafNs = "http://xmlns.com/foaf/0.1/";
    private const string DctermsNs = "http://purl.org/dc/terms/";
    private const string SkosNs = "http://www.w3.org/2004/02/skos/core#";
    private const string SchemaNs = "http://schema.org/";
    private const string IriBaseUrl = "https://register.ld.admin.ch/i14y";
    private const string AgentBaseUri = IriBaseUrl + "/agent/";

    private AgentRdfSerializer _serializer = null!;

    [SetUp]
    public void SetUp() =>
        _serializer = new AgentRdfSerializer(
            Substitute.For<ILogger<AgentRdfSerializer>>(),
            Microsoft.Extensions.Options.Options.Create(new I14YOptions { IriBaseUrl = IriBaseUrl }));

    [Test]
    public void Given_full_agent_When_serializing_to_ttl_Then_organization_is_dual_typed()
    {
        var agent = CreateFullAgent();

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));

        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));
        var rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        graph.ContainsTriple(new Triple(subject, rdfType, graph.CreateUriNode(new Uri($"{OrgNs}Organization"))))
            .Should().BeTrue("the agent must be typed as org:Organization");
        graph.ContainsTriple(new Triple(subject, rdfType, graph.CreateUriNode(new Uri($"{FoafNs}Organization"))))
            .Should().BeTrue("the agent must be typed as foaf:Organization");
    }

    [Test]
    public void Given_full_agent_When_serializing_to_ttl_Then_multilingual_and_scalar_predicates_are_emitted()
    {
        var agent = CreateFullAgent();

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));

        // foaf:name with a German language tag
        var nameLiterals = LiteralsOf(graph, subject, $"{FoafNs}name");
        nameLiterals.Should().Contain(l => l.Value == "Bundesamt für Statistik" && l.Language == "de");

        // skos:prefLabel multilingual
        LiteralsOf(graph, subject, $"{SkosNs}prefLabel").Should().Contain(l => l.Value == "BFS" && l.Language == "de");

        // dcterms:description multilingual
        LiteralsOf(graph, subject, $"{DctermsNs}description").Should().Contain(l => l.Language == "fr");

        // dcterms:identifier holds the plain identifier (and the uid as a second identifier)
        var identifiers = LiteralsOf(graph, subject, $"{DctermsNs}identifier").Select(l => l.Value).ToList();
        identifiers.Should().Contain("CH_BFS");
        identifiers.Should().Contain("CHE-123.456.789");
    }

    [Test]
    public void Given_agent_with_homepage_When_serializing_Then_homepage_is_a_uri_typed_as_foaf_document()
    {
        var agent = CreateFullAgent();

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));
        var rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        var homepageNodes = graph
            .GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode(new Uri($"{FoafNs}homepage")))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .ToList();

        homepageNodes.Should().ContainSingle();
        homepageNodes[0].Uri.Should().Be(new Uri("https://www.bfs.admin.ch"));

        graph.ContainsTriple(new Triple(homepageNodes[0], rdfType, graph.CreateUriNode(new Uri($"{FoafNs}Document"))))
            .Should().BeTrue("the homepage resource must be typed as foaf:Document");
    }

    [Test]
    public void Given_agent_with_classification_When_serializing_Then_org_classification_points_to_register_uri()
    {
        var agent = CreateFullAgent();

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));

        var classificationUris = graph
            .GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode(new Uri($"{OrgNs}classification")))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .Select(n => n.Uri)
            .ToList();

        classificationUris.Should().Contain(new Uri("https://register.ld.admin.ch/i14y/concept/legalForm/0220"));
    }

    [Test]
    public void Given_agent_with_images_When_serializing_Then_schema_image_links_are_emitted()
    {
        var agent = CreateFullAgent();

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));

        var imageUris = graph
            .GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode(new Uri($"{SchemaNs}image")))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .Select(n => n.Uri)
            .ToList();

        imageUris.Should().Contain(new Uri("https://www.bfs.admin.ch/logo.png"));
    }

    [Test]
    public void Given_agent_with_sub_agents_When_serializing_Then_org_hasSubOrganization_links_are_emitted()
    {
        var subAgentId = Guid.NewGuid();
        var agent = CreateFullAgent() with
        {
            SubAgents = [new IdNameModel { Id = subAgentId }]
        };

        var graph = ParseTurtle(_serializer.Serialize([agent], RdfExportFormat.TTL));
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));

        var subOrgUris = graph
            .GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode(new Uri($"{OrgNs}hasSubOrganization")))
            .Select(t => t.Object)
            .OfType<IUriNode>()
            .Select(n => n.Uri)
            .ToList();

        subOrgUris.Should().Contain(new Uri($"{AgentBaseUri}{subAgentId}"));
    }

    [Test]
    public void Given_multiple_agents_When_serializing_Then_all_subjects_are_present_in_one_graph()
    {
        var first = CreateFullAgent();
        var second = CreateMinimalAgent();

        var graph = ParseTurtle(_serializer.Serialize([first, second], RdfExportFormat.TTL));

        var rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));
        var orgType = graph.CreateUriNode(new Uri($"{OrgNs}Organization"));

        var organizationSubjects = graph
            .GetTriplesWithPredicateObject(rdfType, orgType)
            .Select(t => t.Subject)
            .OfType<IUriNode>()
            .Select(n => n.Uri)
            .ToList();

        organizationSubjects.Should().Contain(new Uri($"{AgentBaseUri}{first.Id}"));
        organizationSubjects.Should().Contain(new Uri($"{AgentBaseUri}{second.Id}"));
    }

    [Test]
    public void Given_minimal_agent_When_serializing_Then_no_exception_and_required_fields_present()
    {
        var agent = CreateMinimalAgent();

        var ttl = _serializer.Serialize([agent], RdfExportFormat.TTL);

        var graph = ParseTurtle(ttl);
        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));

        LiteralsOf(graph, subject, $"{DctermsNs}identifier").Select(l => l.Value).Should().Contain("CH_MIN");
    }

    [Test]
    public void Given_full_agent_When_serializing_to_rdfxml_Then_output_is_valid_rdfxml()
    {
        var agent = CreateFullAgent();

        var rdfXml = _serializer.Serialize([agent], RdfExportFormat.RDF);

        using var graph = new Graph();
        var parser = new RdfXmlParser();
        using var reader = new StringReader(rdfXml);
        parser.Load(graph, reader);

        var subject = graph.CreateUriNode(new Uri($"{AgentBaseUri}{agent.Id}"));
        var rdfType = graph.CreateUriNode(UriFactory.Create(RdfSpecsHelper.RdfType));

        graph.ContainsTriple(new Triple(subject, rdfType, graph.CreateUriNode(new Uri($"{FoafNs}Organization"))))
            .Should().BeTrue();
    }

    private static AgentModel CreateFullAgent() => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH_BFS",
        Uid = "CHE-123.456.789",
        Name = new MultiLanguageModel { De = "Bundesamt für Statistik", Fr = "Office fédéral de la statistique" },
        PrefLabel = new MultiLanguageModel { De = "BFS", Fr = "OFS" },
        Description = new MultiLanguageModel { De = "Statistik", Fr = "Statistique" },
        HomePage = "https://www.bfs.admin.ch",
        Classification = new VocabularyEntryModel
        {
            Code = "0220",
            Uri = "https://register.ld.admin.ch/i14y/concept/legalForm/0220"
        },
        ContactPoint = new VCardModel
        {
            HasEmail = "info@bfs.admin.ch",
            HasTelephone = "+41 58 000 00 00",
            Fn = new MultiLanguageModel { De = "Kontakt" }
        },
        Images = [new ResourceModel { Uri = "https://www.bfs.admin.ch/logo.png" }],
        Spatial = ["Schweiz"],
        System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
    };

    private static AgentModel CreateMinimalAgent() => new()
    {
        Id = Guid.NewGuid(),
        Identifier = "CH_MIN",
        Name = new MultiLanguageModel { De = "Minimal" },
        PrefLabel = new MultiLanguageModel { De = "MIN" },
        System = new SystemInfoModel { CreatedAt = DateTimeOffset.UnixEpoch }
    };

    private static Graph ParseTurtle(string ttl)
    {
        var graph = new Graph();
        var parser = new TurtleParser();
        using var reader = new StringReader(ttl);
        parser.Load(graph, reader);
        return graph;
    }

    private static List<ILiteralNode> LiteralsOf(IGraph graph, INode subject, string predicateUri) =>
        graph
            .GetTriplesWithSubjectPredicate(subject, graph.CreateUriNode(new Uri(predicateUri)))
            .Select(t => t.Object)
            .OfType<ILiteralNode>()
            .ToList();
}
