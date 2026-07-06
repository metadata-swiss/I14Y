using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class UpdateDatasetQualityInformationCommandHandler : IRequestHandler<UpdateDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public UpdateDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) => 
        _datasetsService = datasetsService;

    public Task Handle(UpdateDatasetQualityInformationCommand request, CancellationToken cancellationToken)
    {
        return _datasetsService.UpdateDatasetQualityInformation(request.DatasetId, request.Model, cancellationToken);
    }
}
