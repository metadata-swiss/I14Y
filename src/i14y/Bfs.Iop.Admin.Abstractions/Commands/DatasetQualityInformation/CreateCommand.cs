using Bfs.Iop.Admin.Models;
using MediatR;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class CreateCommand : IRequest<DatasetQualityInformationData>
{
    public CreateCommand(DatasetQualityInformationData qualityInformations)
        => DatasetQualityInformationData = qualityInformations;

    public DatasetQualityInformationData DatasetQualityInformationData { get; }
}