using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class CreateDatasetModelClassCommandHandler : IRequestHandler<CreateDatasetModelClassCommand, Uri>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public CreateDatasetModelClassCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task<Uri> Handle(CreateDatasetModelClassCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.CreateSchemaClass(request.DatasetId, request.SchemaClassInput, cancellationToken);
    }
}
