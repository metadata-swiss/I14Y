using Bfs.Iop.Core.Abstractions.Commands.DatasetQualityInformation;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.DatasetQualityInformation;

internal sealed class DeleteDatasetQualityInformationCommandHandler : IRequestHandler<DeleteDatasetQualityInformationCommand>
{
    private readonly IDatasetsService _datasetsService;

    public DeleteDatasetQualityInformationCommandHandler(IDatasetsService datasetsService) 
        => _datasetsService = datasetsService;

    public Task Handle(DeleteDatasetQualityInformationCommand request, CancellationToken cancellationToken)
    {
        return _datasetsService.DeleteDatasetQualityInformation(request.DatasetId, cancellationToken);
    }
}
