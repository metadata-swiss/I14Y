using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Api.Formatters;

/// <summary>
/// Serializes agents to RDF/XML (<c>application/rdf+xml</c>).
/// </summary>
internal sealed class RdfXmlOutputFormatter : AgentRdfOutputFormatter
{
    public RdfXmlOutputFormatter()
        : base(CatalogExportFormat.RDF, "application/rdf+xml")
    {
    }
}
