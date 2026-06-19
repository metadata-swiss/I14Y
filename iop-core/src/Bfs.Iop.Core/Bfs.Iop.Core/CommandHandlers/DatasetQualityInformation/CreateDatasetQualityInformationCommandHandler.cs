using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class CreateDatasetQualityInformationCommandHandler : IRequestHandler<CreateDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public CreateDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService;

    public Task Handle(CreateDatasetQualityInformationCommand request, CancellationToken cancellationToken)
    {
        return _datasetsService.AddDatasetQualityInformation(request.DatasetId, request.DatasetQualityInformationDataModel, cancellationToken);
    }
}
