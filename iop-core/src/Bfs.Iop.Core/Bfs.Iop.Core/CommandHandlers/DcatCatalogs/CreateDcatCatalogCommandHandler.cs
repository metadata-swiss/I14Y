using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogCommandHandler : IRequestHandler<CreateDcatCatalogCommand, Guid>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public CreateDcatCatalogCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<Guid> Handle(CreateDcatCatalogCommand request, CancellationToken cancellationToken) =>
        _dcatCatalogsService.AddDcatCatalog(request.InputModel, cancellationToken);
}
