using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class UpdateCommand : IRequest
{
    public UpdateCommand(DatasetQualityInformationData qualityInformations)
        => DatasetQualityInformationData = qualityInformations;

    public DatasetQualityInformationData DatasetQualityInformationData { get; }
}