using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class ExportDatasetModelCommandHandler : IRequestHandler<ExportDatasetModelCommand, ExportFile>
{
    private readonly IDatasetModelProcessService _datasetModelFileProcessService;

    public ExportDatasetModelCommandHandler(
        IDatasetModelProcessService datasetModelFileProcessService) => _datasetModelFileProcessService = datasetModelFileProcessService
            ?? throw new ArgumentNullException(nameof(datasetModelFileProcessService));

    public async Task<ExportFile> Handle(ExportDatasetModelCommand request, CancellationToken cancellationToken) =>
        await _datasetModelFileProcessService.ExportGraph(request.Format, request.DatasetId, cancellationToken);
}
