using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class GetDcatCatalogCommandHandler : IRequestHandler<GetDcatCatalogCommand, DcatCatalogModel>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public GetDcatCatalogCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<DcatCatalogModel> Handle(GetDcatCatalogCommand request, CancellationToken cancellationToken) =>
        _dcatCatalogsService.GetDcatCatalog(request.Id, cancellationToken);
}
