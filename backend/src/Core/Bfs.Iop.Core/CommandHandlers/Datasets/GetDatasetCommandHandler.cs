using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class GetDatasetCommandHandler : IRequestHandler<GetDatasetCommand, DcatDatasetModel>
{
    private readonly IDatasetsService _datasetsService;

    public GetDatasetCommandHandler(IDatasetsService datasetsService) =>
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

    public Task<DcatDatasetModel> Handle(GetDatasetCommand request, CancellationToken cancellationToken) =>
        _datasetsService.GetDataset(request.DatasetId, cancellationToken);
}
