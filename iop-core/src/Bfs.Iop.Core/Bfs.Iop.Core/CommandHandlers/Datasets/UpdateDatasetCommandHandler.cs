using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class UpdateDatasetCommandHandler : IRequestHandler<UpdateDatasetCommand>
{
    private readonly ILogger<UpdateDatasetCommandHandler> _logger;
    private readonly IDatasetsService _datasetsService;

    public UpdateDatasetCommandHandler(
        ILogger<UpdateDatasetCommandHandler> logger,
        IDatasetsService datasetsService)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
    }

    public Task Handle(UpdateDatasetCommand request, CancellationToken cancellationToken)
    {
        _logger.LogTrace("Update dataset with id '{Id}'", request.DatasetId);

        return _datasetsService.UpdateDataset(request.DatasetId, request.DatasetInput, cancellationToken);
    }
}
