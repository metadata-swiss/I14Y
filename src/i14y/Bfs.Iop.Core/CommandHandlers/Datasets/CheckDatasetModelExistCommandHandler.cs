using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class CheckDatasetModelExistCommandHandler : IRequestHandler<CheckDatasetModelExistCommand,bool>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;

    public CheckDatasetModelExistCommandHandler(IDatasetModelProcessService datasetModelFileProcessService) => 
        _datasetModelFileProcessService = datasetModelFileProcessService ?? 
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

    public Task<bool> Handle(CheckDatasetModelExistCommand request, CancellationToken cancellationToken) => 
        _datasetModelFileProcessService.GraphExists(request.DatasetId, cancellationToken);
}
