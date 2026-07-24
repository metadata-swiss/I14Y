using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Core.Api.Formatters;

/// <summary>
/// Serializes agents to Turtle (<c>text/turtle</c>, <c>application/x-turtle</c>).
/// </summary>
internal sealed class TurtleOutputFormatter : AgentRdfOutputFormatter
{
    public TurtleOutputFormatter()
        : base(CatalogExportFormat.TTL, "text/turtle", "application/x-turtle")
    {
    }
}
