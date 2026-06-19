using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class UpdateDatasetModelPositionCommandHandler : IRequestHandler<UpdateDatasetModelPositionCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public UpdateDatasetModelPositionCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task Handle(UpdateDatasetModelPositionCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.UpdateClassesPosition(request.DatasetId, request.ClassesPositionInput, cancellationToken);
    }
}
