using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Abstractions.Models.LinkedData;
using Bfs.Iop.Core.LinkedData.Services;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDatasetModelGraphCommandHandler : IRequestHandler<GetDatasetModelGraphCommand, SchemaGraph>
{
    private readonly IDatasetModelProcessService _datasetModelProcessService;

    public GetDatasetModelGraphCommandHandler(IDatasetModelProcessService datasetModelHandleService) =>
        _datasetModelProcessService = datasetModelHandleService
            ?? throw new ArgumentNullException(nameof(datasetModelHandleService));

    public async Task<SchemaGraph> Handle(GetDatasetModelGraphCommand request, CancellationToken cancellationToken) => 
        await _datasetModelProcessService.GetDatasetModelGraphAsync(request.DatasetId, cancellationToken);
}
