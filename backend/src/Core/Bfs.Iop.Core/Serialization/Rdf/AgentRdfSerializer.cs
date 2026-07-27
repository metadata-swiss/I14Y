using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.CommandHandlers.DcatCatalogs.Extensions;
using Bfs.Iop.Core.Settings;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text;
using VDS.RDF;
using VDS.RDF.Writing;

namespace Bfs.Iop.Core.Serialization.Rdf;

/// <summary>
/// Builds an RDF graph of agents (organizations) following the opendata.swiss / Piveau
/// organization representation (see the ch-are.ttl reference sample):
/// each agent is typed as <c>org:Organization, foaf:Organization</c> and carries multilingual
/// <c>foaf:name</c> / <c>skos:prefLabel</c> / <c>dcterms:description</c>, a plain <c>dcterms:identifier</c>,
/// a <c>foaf:homepage</c> and an <c>org:classification</c> (legal form).
/// </summary>
internal sealed class AgentRdfSerializer : IAgentRdfSerializer
{
    private const string OrgNamespace = "http://www.w3.org/ns/org#";
    private const string FoafNamespace = "http://xmlns.com/foaf/0.1/";
    private const string DublinCoreNamespace = "http://purl.org/dc/terms/";
    private const string SkosNamespace = "http://www.w3.org/2004/02/skos/core#";
    private const string DcatNamespace = "http://www.w3.org/ns/dcat#";
    private const string VcardNamespace = "http://www.w3.org/2006/vcard/ns#";
    private const string SchemaOrgNamespace = "http://schema.org/";

    private static readonly string[] _allowedUriSchemes = [Uri.UriSchemeHttp, Uri.UriSchemeHttps];

    private readonly ILogger<AgentRdfSerializer> _logger;
    private readonly string _agentBaseUri;

    public AgentRdfSerializer(ILogger<AgentRdfSerializer> logger, IOptions<I14YOptions> i14yOptions)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        ArgumentNullException.ThrowIfNull(i14yOptions, nameof(i14yOptions));
        _agentBaseUri = $"{i14yOptions.Value.IriBaseUrl.TrimEnd('/')}/agent/";
    }

    public string Serialize(IEnumerable<AgentModel> agents, RdfExportFormat format)
    {
        ArgumentNullException.ThrowIfNull(agents, nameof(agents));

        using var graph = new Graph();

        graph.NamespaceMap.AddNamespace("org", new Uri(OrgNamespace));
        graph.NamespaceMap.AddNamespace("foaf", new Uri(FoafNamespace));
        graph.NamespaceMap.AddNamespace("dcterms", new Uri(DublinCoreNamespace));
        graph.NamespaceMap.AddNamespace("skos", new Uri(SkosNamespace));
        graph.NamespaceMap.AddNamespace("dcat", new Uri(DcatNamespace));
        graph.NamespaceMap.AddNamespace("vcard", new Uri(VcardNamespace));
        graph.NamespaceMap.AddNamespace("schema", new Uri(SchemaOrgNamespace));

        var rdfType = graph.CreateUriNode("rdf:type");

        foreach (var agent in agents)
        {
            AddAgent(graph, rdfType, agent);
        }

        return WriteOutput(graph, format);
    }

    private void AddAgent(Graph graph, IUriNode rdfType, AgentModel agent)
    {
        var agentUri = graph.CreateUriNode(new Uri($"{_agentBaseUri}{agent.Id}", UriKind.Absolute));

        // dual-typed as org:Organization and foaf:Organization (matches the ch-are.ttl sample)
        graph.AssertUri(agentUri, rdfType, "org:Organization");
        graph.AssertUri(agentUri, rdfType, "foaf:Organization");

        graph.AssertLiteral(agentUri, "dcterms:identifier", agent.Identifier);

        if (!string.IsNullOrWhiteSpace(agent.Uid))
        {
            graph.AssertLiteral(agentUri, "dcterms:identifier", agent.Uid);
        }

        graph.Assert(agentUri, "foaf:name", agent.Name);
        graph.Assert(agentUri, "skos:prefLabel", agent.PrefLabel);

        if (agent.Description is not null)
        {
            graph.Assert(agentUri, "dcterms:description", agent.Description);
        }

        if (!string.IsNullOrWhiteSpace(agent.HomePage)
            && TryCreateUriNode(graph, agent.HomePage, "foaf:homepage", out var homePageNode))
        {
            graph.Assert(agentUri, "foaf:homepage", homePageNode);
            graph.AssertUri(homePageNode, rdfType, "foaf:Document");
        }

        if (agent.Classification?.Uri is { } classificationUri
            && TryCreateUriNode(graph, classificationUri, "org:classification", out var classificationNode))
        {
            graph.Assert(agentUri, "org:classification", classificationNode);
        }

        // Images (schema:image, as in DCAT-AP CH; I14Y extension beyond the minimal sample).
        foreach (var image in agent.Images)
        {
            if (TryCreateUriNode(graph, image.Uri, "schema:image", out var imageNode))
            {
                graph.Assert(agentUri, "schema:image", imageNode);
            }
        }

        // Spatial coverage (I14Y extension beyond the minimal sample).
        foreach (var spatialCh in agent.SpatialCH)
        {
            if (spatialCh.Uri is { } spatialUri
                && TryCreateUriNode(graph, spatialUri, "dcterms:spatial", out var spatialNode))
            {
                graph.Assert(agentUri, "dcterms:spatial", spatialNode);
            }
        }

        foreach (var spatial in agent.Spatial.Where(x => !string.IsNullOrWhiteSpace(x)))
        {
            graph.AssertLiteral(agentUri, "dcterms:spatial", spatial);
        }

        if (agent.ContactPoint is not null)
        {
            AssertVCard(graph, rdfType, agentUri, agent.ContactPoint);
        }

        // Sub-agent relations (I14Y extension).
        foreach (var subAgent in agent.SubAgents)
        {
            var subAgentUri = graph.CreateUriNode(new Uri($"{_agentBaseUri}{subAgent.Id}", UriKind.Absolute));
            graph.Assert(agentUri, "org:hasSubOrganization", subAgentUri);
        }
    }

    private static void AssertVCard(Graph graph, IUriNode rdfType, INode subj, VCardModel vcard)
    {
        var contact = graph.CreateBlankNode();
        graph.Assert(subj, "dcat:contactPoint", contact);
        graph.AssertUri(contact, rdfType, "vcard:Organization");

        if (vcard.Fn is not null)
        {
            graph.Assert(contact, "vcard:fn", vcard.Fn);
        }

        if (vcard.HasAddress is not null)
        {
            graph.Assert(contact, "vcard:adrWork", vcard.HasAddress);
        }

        if (vcard.Note is not null)
        {
            graph.Assert(contact, "vcard:note", vcard.Note);
        }

        graph.AssertLiteral(contact, "vcard:hasEmail", vcard.HasEmail);

        if (!string.IsNullOrWhiteSpace(vcard.HasTelephone))
        {
            graph.AssertLiteral(contact, "vcard:hasTelephone", vcard.HasTelephone);
        }
    }

    private bool TryCreateUriNode(Graph graph, string uri, string usedAs, out IUriNode node)
    {
        if (Uri.TryCreate(uri, UriKind.Absolute, out var parsed) && _allowedUriSchemes.Contains(parsed.Scheme))
        {
            node = graph.CreateUriNode(parsed);
            return true;
        }

        _logger.LogWarning("'{uri}' is not a valid absolute http(s) URI used as {usedAs} and was skipped.", uri, usedAs);
        node = null!;
        return false;
    }

    private static string WriteOutput(IGraph graph, RdfExportFormat format)
    {
        // Creating a StreamWriter with UTF-8 encoding:
        // As StringWriter does not offer the option of specifying the encoding directly (it uses UTF-16 by default,
        // as this is the internal representation of .NET strings)
        using var memoryStream = new MemoryStream();
        using (var streamWriter = new StreamWriter(memoryStream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
        {
            switch (format)
            {
                case RdfExportFormat.RDF:
                    var rdfXmlWriter = new PrettyRdfXmlWriter() { PrettyPrintMode = true };
                    rdfXmlWriter.Save(graph, streamWriter);
                    break;

                case RdfExportFormat.TTL:
                    var ttlWriter = new CompressingTurtleWriter();
                    ttlWriter.Save(graph, streamWriter);
                    break;

                default:
                    throw new NotSupportedException($"The format '{format}' is not supported.");
            }

            streamWriter.Flush();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        using var streamReader = new StreamReader(memoryStream, Encoding.UTF8);
        var result = streamReader.ReadToEnd();

        if (format == RdfExportFormat.RDF)
        {
            result = result.Replace("encoding=\"utf-16\"", "encoding=\"utf-8\"", StringComparison.OrdinalIgnoreCase);
        }

        return result;
    }
}
