using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class UpdateDatasetModelPropertyCommandHandler : IRequestHandler<UpdateDatasetModelPropertyCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public UpdateDatasetModelPropertyCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task Handle(UpdateDatasetModelPropertyCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.UpdateSchemaProperty(
            request.DatasetId,
            request.PropertyInput,
            request.ClassUri,
            cancellationToken);
    }
}
