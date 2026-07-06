using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class UpdateDcatCatalogCommandHandler : IRequestHandler<UpdateDcatCatalogCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public UpdateDcatCatalogCommandHandler(IDcatCatalogsService dcatCatalogsService) =>
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task Handle(UpdateDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        return _dcatCatalogsService.UpdateDcatCatalog(request.Id, request.UpdateModel, cancellationToken);
    }
}
