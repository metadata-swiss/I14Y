using Bfs.Iop.Admin.Models;
using MediatR;
using System;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class GetByDatasetIdCommand : IRequest<DatasetQualityInformationData>
{
    public GetByDatasetIdCommand(Guid datasetId)
        => DatasetId = datasetId;

    public Guid DatasetId { get; }
}