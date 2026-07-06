using MediatR;
using System;
using System.Collections.Generic;

namespace Bfs.Iop.Admin.Commands.DistributionSummary;

public class GetAllByDatasetIdCommand : IRequest<IEnumerable<Models.DistributionSummary>>
{
    public GetAllByDatasetIdCommand(Guid datasetId)
        => DatasetId = datasetId;

    public Guid DatasetId { get; }
}