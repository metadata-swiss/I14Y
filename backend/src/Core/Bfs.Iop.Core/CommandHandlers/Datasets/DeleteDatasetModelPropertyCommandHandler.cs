using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class DeleteDatasetModelPropertyCommandHandler : IRequestHandler<DeleteDatasetModelPropertyCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public DeleteDatasetModelPropertyCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task Handle(DeleteDatasetModelPropertyCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.DeleteSchemaProperty(request.DatasetId, request.PropertyUri, cancellationToken);
    }
}
