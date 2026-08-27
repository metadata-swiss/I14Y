using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Common.Exceptions;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class DeleteDatasetModelCommandHandler : IRequestHandler<DeleteDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexWriter _catalogIndexWriter;

    public DeleteDatasetModelCommandHandler(
        IDatasetsService datasetsService,
        IDatasetModelProcessService datasetModelFileProcessService,
        ICatalogIndexWriter catalogIndexWriter)
    {
        _datasetsService = datasetsService ??
            throw new ArgumentNullException(nameof(datasetsService));

        _datasetModelFileProcessService = datasetModelFileProcessService ?? 
            throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _catalogIndexWriter = catalogIndexWriter ?? 
            throw new ArgumentNullException(nameof(catalogIndexWriter));
    }

    public async Task Handle(DeleteDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _datasetModelFileProcessService.DeleteGraph(request.DatasetId, cancellationToken);

        // Update index
        var dataset = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        await _catalogIndexWriter.UpdateIndexAsync(dataset, hasStructure: false, cancellationToken);

        return;
    }
}
