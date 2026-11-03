using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.DataAccess.Contracts;
using Cerise;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class CreateDatasetQualityInformationCommandHandler : AsyncRequestHandler<CreateDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public CreateDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService;

    protected override Task Handle(CreateDatasetQualityInformationCommand request, CancellationToken cancellationToken) =>
        _datasetsService.AddDatasetQualityInformation(request.DatasetId, request.DatasetQualityInformationDataModel, cancellationToken);
}
