using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Lucene.Index;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class CreateDatasetCommandHandler : IRequestHandler<CreateDatasetCommand, Guid>
{
    private readonly IDatasetsService _datasetsService;
    private readonly ICatalogIndexService _catalogIndexService;

    public CreateDatasetCommandHandler(
        IDatasetsService datasetsService,
        ICatalogIndexService catalogIndexService)
    {
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));
        _catalogIndexService = catalogIndexService ?? throw new ArgumentNullException(nameof(catalogIndexService));
    }

    public async Task<Guid> Handle(CreateDatasetCommand request, CancellationToken cancellationToken)
    {
        var id = await _datasetsService.AddDataset(request.DatasetInput, cancellationToken);

        var resource = await _datasetsService.GetDataset(id, cancellationToken);

        _catalogIndexService.UpdateIndex(resource, hasStructure: false);

        return id;
    }
}
