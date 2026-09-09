using Bfs.Iop.Core.Abstractions.Commands.DcatCatalogs;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DcatCatalogs;

internal sealed class CreateDcatCatalogRecordsCommandHandler : IRequestHandler<CreateDcatCatalogRecordsCommand, IEnumerable<Guid>>
{
    private readonly IDcatCatalogsService _dcatCatalogsService;

    public CreateDcatCatalogRecordsCommandHandler(IDcatCatalogsService dcatCatalogsService) => 
        _dcatCatalogsService = dcatCatalogsService ?? throw new ArgumentNullException(nameof(dcatCatalogsService));

    public Task<IEnumerable<Guid>> Handle(CreateDcatCatalogRecordsCommand request, CancellationToken cancellationToken) =>
        _dcatCatalogsService.AddDcatCatalogRecords(request.DcatCatalogId, request.InputModels, cancellationToken);
}