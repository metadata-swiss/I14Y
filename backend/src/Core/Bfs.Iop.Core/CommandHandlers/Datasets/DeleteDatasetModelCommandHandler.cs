using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetModelCommandHandler : IRequestHandler<DeleteDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly ICatalogIndexService _catalogIndexService;

    public DeleteDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        ICatalogIndexService catalogIndexService)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService ?? 
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(DeleteDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);

        _catalogIndexService.DeIndex(request.DatasetId);

        return;
    }
}
