using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.Core.Services.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ImportDatasetModelCommandHandler : IRequestHandler<ImportDatasetModelCommand>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexWriter _catalogIndexWriter;

    public ImportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IDatasetsService datasetsService,
        ICatalogIndexWriter catalogIndexWriter)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

        _catalogIndexWriter = catalogIndexWriter ?? throw new ArgumentNullException(nameof(catalogIndexWriter));
    }

    public async Task Handle(ImportDatasetModelCommand request, CancellationToken cancellationToken)
    {
        await _datasetModelFileProcessService.UploadGraph(request.ImportFile, request.DatasetId, cancellationToken);

        // Update index
        var dataset = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        await _catalogIndexWriter.UpdateIndexAsync(dataset, hasStructure: true, cancellationToken);

        return;
    }
}