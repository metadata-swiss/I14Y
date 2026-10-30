using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class DeleteDatasetQualityInformationCommandHandler : AsyncRequestHandler<DeleteDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public DeleteDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) 
        => _datasetsService = datasetsService;

    protected override Task Handle(DeleteDatasetQualityInformationCommand request, CancellationToken cancellationToken)
    {
        return _datasetsService.DeleteDatasetQualityInformation(request.DatasetId, cancellationToken);
    }
}
