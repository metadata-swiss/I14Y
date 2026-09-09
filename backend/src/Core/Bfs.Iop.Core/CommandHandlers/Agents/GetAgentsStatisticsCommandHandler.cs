using Bfs.Iop.Core.Abstractions.Commands.Agents;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Agents;

internal sealed class GetAgentsStatisticsCommandHandler : IRequestHandler<GetAgentsStatisticsCommand, IEnumerable<AgentStatisticsResult>>
{
    private readonly IAgentsService _agentsService;
    private readonly IDatasetsService _datasetsService;
    private readonly IDataServicesService _dataServicesService;
    private readonly IIopConceptsService _conceptsService;
    private readonly IPublicServicesService _publicServicesService;
    private readonly IMappingTablesService _mappingTablesService;

    public GetAgentsStatisticsCommandHandler(
        IAgentsService agentsService, 
        IDatasetsService datasetsService,
        IDataServicesService dataServicesService,
        IIopConceptsService conceptsService,
        IPublicServicesService publicServicesService,
        IMappingTablesService mappingTablesService)
    {
        _agentsService = agentsService ?? throw new ArgumentNullException(nameof(agentsService));
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _dataServicesService = dataServicesService ?? throw new ArgumentNullException(nameof(datasetsService));
        _conceptsService = conceptsService ?? throw new ArgumentNullException(nameof(conceptsService));
        _publicServicesService = publicServicesService ?? throw new ArgumentNullException(nameof(publicServicesService));
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));
    }

    public async Task<IEnumerable<AgentStatisticsResult>> Handle(GetAgentsStatisticsCommand request, CancellationToken cancellationToken)
    {
        var agents = (await _agentsService.GetAgents(identifier: null, uid: null, page: 1, pageSize: int.MaxValue, cancellationToken)).Results;

        var statistics = new List<IEnumerable<AgentStatisticsResult>>()
        {
            _datasetsService.GetPublishersStatistics(agents, cancellationToken).GetAwaiter().GetResult(),
            _dataServicesService.GetPublishersStatistics(agents, cancellationToken).GetAwaiter().GetResult(),
            _conceptsService.GetPublishersStatistics(agents, cancellationToken).GetAwaiter().GetResult(),
            _publicServicesService.GetPublishersStatistics(agents, cancellationToken).GetAwaiter().GetResult(),
            _mappingTablesService.GetPublishersStatistics(agents, cancellationToken).GetAwaiter().GetResult()
        };

        var result = agents.Select(agent => new AgentStatisticsResult()
        {
            Publisher = agent,
            Types = [.. statistics
                .SelectMany(x => x)
                .Where(x => x.Publisher.Id == agent.Id)
                .SelectMany(x => x.Types)]
        });

        return result;
    }
}
