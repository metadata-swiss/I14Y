using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class DeleteByDatasetIdCommand : IRequest
{
    public DeleteByDatasetIdCommand(Guid datasetId)
        => DatasetId = datasetId;

    public Guid DatasetId { get; }
}