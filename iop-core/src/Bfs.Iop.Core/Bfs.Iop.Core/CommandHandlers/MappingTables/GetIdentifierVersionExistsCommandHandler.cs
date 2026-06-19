using Bfs.Iop.Core.Abstractions.Commands.MappingTables;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.MappingTables;

internal sealed class GetIdentifierVersionExistsCommandHandler : IRequestHandler<GetIdentifierVersionExistsCommand, IdentifierVersionExistsModel>
{
    private readonly IMappingTablesService _mappingTablesService;

    public GetIdentifierVersionExistsCommandHandler(IMappingTablesService mappingTablesService) =>
        _mappingTablesService = mappingTablesService ?? throw new ArgumentNullException(nameof(mappingTablesService));

    public Task<IdentifierVersionExistsModel> Handle(GetIdentifierVersionExistsCommand request, CancellationToken cancellationToken)
    {
        // we need to have the information about objects that user have no rights to see.
        return _mappingTablesService.GetIdentifierVersionExists(request.Identifier, request.Version, cancellationToken);
    }
}
