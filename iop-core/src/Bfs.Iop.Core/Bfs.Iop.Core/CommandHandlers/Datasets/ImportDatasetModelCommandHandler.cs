using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Lucene.Index;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ImportDatasetModelCommandHandler : IRequestHandler<ImportDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public ImportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(ImportDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _datasetModelFileProcessService.UploadGraph(request.ImportFile, request.DatasetId, cancellationToken);

        // Update index
        var dataset = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        _catalogIndexService.UpdateIndex(dataset, hasStructure: true);

        return;
    }
}