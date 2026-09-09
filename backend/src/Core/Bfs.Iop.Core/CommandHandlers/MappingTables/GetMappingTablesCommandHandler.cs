using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class GetMappingTablesCommandHandler : IRequestHandler<GetMappingTablesCommand, PagedResult<MappingTableModel>>
{
    private readonly IMappingTablesService _mappingTablesService;

    public GetMappingTablesCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<PagedResult<MappingTableModel>> Handle(GetMappingTablesCommand request, CancellationToken cancellationToken)
    {
        (var page, var pageSize) = request.Page.HasValue && request.PageSize.HasValue
            ? (request.Page.Value, request.PageSize.Value)
            : (1, int.MaxValue);

        return _mappingTablesService.GetMappingTables(
            request.MappingTableIdentifier,
            request.PublisherIdentifier,
            request.Version,
            request.CodeSystemUri,
            request.PublicationLevel,
            request.RegistrationStatus,
            page,
            pageSize, 
            cancellationToken);
    }
}
