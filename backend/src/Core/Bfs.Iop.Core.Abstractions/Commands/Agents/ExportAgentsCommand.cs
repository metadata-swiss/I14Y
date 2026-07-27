using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Agents;

/// <summary>
/// Exports all agents (organisations) as a single RDF graph in the requested format.
/// </summary>
public sealed record ExportAgentsCommand(RdfExportFormat Format) : IRequest<string>
{ }
