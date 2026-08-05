using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Serialization.Rdf;

/// <summary>
/// Serializes agents (organizations) into RDF (Turtle or RDF/XML), compatible with the
/// opendata.swiss / Piveau organization representation.
/// </summary>
public interface IAgentRdfSerializer
{
    /// <summary>
    /// Serializes the given agents into a single RDF graph using the requested format.
    /// A single agent is serialized by passing a one-element sequence.
    /// </summary>
    string Serialize(IEnumerable<AgentModel> agents, RdfExportFormat format);
}
