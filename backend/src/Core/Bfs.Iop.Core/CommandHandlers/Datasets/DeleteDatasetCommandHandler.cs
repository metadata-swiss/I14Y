using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetCommandHandler : IRequestHandler<DeleteDatasetCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;

    public DeleteDatasetCommandHandler(
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService,
        IDatasetModelProcessService datasetModelFileProcessService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

        _datasetModelFileProcessService = datasetModelFileProcessService ??
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(DeleteDatasetCommand request, CancellationToken cancellationToken)
    {
        await _datasetsService.DeleteDataset(request.DatasetId, cancellationToken);

        if (await _datasetModelFileProcessService.GraphExists(request.DatasetId, cancellationToken))
        {
            await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);
        }

        _catalogIndexService.DeIndex(request.DatasetId);

        return;
    }
}
