using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.Core.Serialization.Rdf;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class ExportAgentsCommandHandler : IRequestHandler<ExportAgentsCommand, string>
{
    private readonly IAgentsService _agentsService;
    private readonly IAgentRdfSerializer _serializer;

    public ExportAgentsCommandHandler(IAgentsService agentsService, IAgentRdfSerializer serializer)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    public async Task<string> Handle(ExportAgentsCommand request, CancellationToken cancellationToken)
    {
        var agents = await _agentsService.GetAgents(
            identifier: null,
            uid: null,
            page: 1,
            pageSize: int.MaxValue,
            cancellationToken);

        return _serializer.Serialize(agents.Results, request.Format);
    }
}
