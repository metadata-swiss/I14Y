using Bfs.Iop.Core.Abstractions.Commands.Datasets;
using Bfs.Iop.Core.Data.Contracts;
using MediatR;

namespace Bfs.Iop.Core.CommandHandlers.Datasets;

internal sealed class CreateDatasetCommandHandler : IRequestHandler<CreateDatasetCommand, Guid>
{
    private readonly IDatasetsService _datasetsService;

    public CreateDatasetCommandHandler(IDatasetsService datasetsService) =>
        _datasetsService = datasetsService ?? throw new ArgumentNullException(nameof(datasetsService));

    public Task<Guid> Handle(CreateDatasetCommand request, CancellationToken cancellationToken) =>
        _datasetsService.AddDataset(request.DatasetInput, cancellationToken);
}
