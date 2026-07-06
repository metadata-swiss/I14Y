using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetView;

public class GetByIdCommand : IRequest<Models.Dataset>
{
    public GetByIdCommand(Guid datasetId)
        => DatasetId = datasetId;

    public Guid DatasetId { get; }
}