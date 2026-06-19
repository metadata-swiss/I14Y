using Bfs.Iop.Core.Abstractions.Models;
using MediatR;

namespace Bfs.Iop.Core.Abstractions.Commands.Datasets;

public sealed record CreateDatasetCommand : IRequest<Guid>
{
    public CreateDatasetCommand(DcatDatasetInputModel datasetInput)
    {
        // ToDo: do it properly after refactoring Datasets validation.
        DatasetInput = datasetInput with
        {
            Distributions = [.. datasetInput.Distributions.Select(d => d with { Id = null })]
        };
    }

    public DcatDatasetInputModel DatasetInput { get; }
}
