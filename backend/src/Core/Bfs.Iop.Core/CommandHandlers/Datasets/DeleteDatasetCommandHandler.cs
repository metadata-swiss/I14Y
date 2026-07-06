using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetCommandHandler : IRequestHandler<DeleteDatasetCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;

    public DeleteDatasetCommandHandler(
        IDatasetsService datasetsService,
        IDatasetModelProcessService datasetModelFileProcessService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

        _datasetModelFileProcessService = datasetModelFileProcessService ??
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));
    }

    public async Task Handle(DeleteDatasetCommand request, CancellationToken cancellationToken)
    {
        await _datasetsService.DeleteDataset(request.DatasetId, cancellationToken);

        if (await _datasetModelFileProcessService.GraphExists(request.DatasetId, cancellationToken))
        {
            await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);
        }

        return;
    }
}
