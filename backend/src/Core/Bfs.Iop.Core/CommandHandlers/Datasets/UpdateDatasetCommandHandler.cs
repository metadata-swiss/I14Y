using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class UpdateDatasetCommandHandler : IRequestHandler<UpdateDatasetCommand>
{
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public UpdateDatasetCommandHandler(
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task Handle(UpdateDatasetCommand request, CancellationToken cancellationToken)
    {
        await _datasetsService.UpdateDataset(request.DatasetId, request.DatasetInput, cancellationToken);

        var resource = await _datasetsService.GetDataset(request.DatasetId, cancellationToken);

        _catalogIndexService.UpdateIndex(resource);
    }
}
