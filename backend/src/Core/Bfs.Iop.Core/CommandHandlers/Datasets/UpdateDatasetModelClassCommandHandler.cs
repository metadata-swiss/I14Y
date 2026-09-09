using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class UpdateDatasetModelClassCommandHandler : IRequestHandler<UpdateDatasetModelClassCommand>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public UpdateDatasetModelClassCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task Handle(UpdateDatasetModelClassCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.UpdateSchemaClass(request.DatasetId, request.SchemaClassInput, cancellationToken);
    }
}
