using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.LinkedData.Services;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ExportDatasetModelCommandHandler : IRequestHandler<ExportDatasetModelCommand, ExportFile>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;
    private readonly IDatasetsService _datasetsService;

    public ExportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService,
        IDatasetsService datasetsService)
    {
        _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
    }

    public async Task<ExportFile> Handle(ExportDatasetModelCommand request, CancellationToken cancellationToken)
    {
        // Ensure the user can read the dataset
        await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        return await _datasetModelFileProcessService.ExportGraph(request.Format, request.DatasetId, cancellationToken);
    }
}
