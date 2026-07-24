using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal class CreateDatasetModelPropertyCommandHandler : IRequestHandler<CreateDatasetModelPropertyCommand, Uri>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public CreateDatasetModelPropertyCommandHandler(
        IDatasetModelProcessService datasetModelProcessService)
    {
        _datasetModelProcessService = datasetModelProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelProcessService));
    }

    public Task<Uri> Handle(CreateDatasetModelPropertyCommand request, CancellationToken cancellationToken)
    {
        return _datasetModelProcessService.CreateSchemaProperty(
            request.DatasetId,          
            request.PropertyInput,
            request.ClassUri,
            cancellationToken);
    }
}
