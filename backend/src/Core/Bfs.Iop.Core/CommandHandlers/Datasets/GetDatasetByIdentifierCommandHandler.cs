using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDatasetByIdentifierCommandHandler : IRequestHandler<GetDatasetByIdentifierCommand, DcatDatasetModel>
{
    private readonly IDatasetsService _datasetsService;

    public GetDatasetByIdentifierCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

    public Task<DcatDatasetModel> Handle(GetDatasetByIdentifierCommand request, CancellationToken cancellationToken)
        => _datasetsService.GetDataset(request.Identifier, cancellationToken);
}
