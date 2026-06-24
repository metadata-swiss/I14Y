using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogRecordCommandHandler : IRequestHandler<CreateDcatCatalogRecordCommand, Guid>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public CreateDcatCatalogRecordCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<Guid> Handle(CreateDcatCatalogRecordCommand request, CancellationToken cancellationToken) =>
        _dcatCatalogsService.AddDcatCatalogRecord(request.DcatCatalogId, request.InputModel, cancellationToken);
}
