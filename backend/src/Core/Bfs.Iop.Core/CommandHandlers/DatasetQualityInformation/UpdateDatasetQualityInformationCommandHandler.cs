using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class UpdateDatasetQualityInformationCommandHandler : AsyncRequestHandler<UpdateDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public UpdateDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService;

    protected override Task Handle(UpdateDatasetQualityInformationCommand request, CancellationToken cancellationToken)
    {
        return _datasetsService.UpdateDatasetQualityInformation(request.DatasetId, request.Model, cancellationToken);
    }
}
