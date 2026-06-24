using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class DeleteDcatCatalogCommandHandler : IRequestHandler<DeleteDcatCatalogCommand>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public DeleteDcatCatalogCommandHandler(IDcatCatalogsService dcatCatalogsService) =>
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task Handle(DeleteDcatCatalogCommand request, CancellationToken cancellationToken)
    {
        return _dcatCatalogsService.DeleteDcatCatalog(request.Id, cancellationToken);
    }
}
