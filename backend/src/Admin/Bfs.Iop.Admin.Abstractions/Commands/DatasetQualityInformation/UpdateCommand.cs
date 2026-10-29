using Bfs.Iop.Admin.Models;
using Cerise;

namespace Bfs.Iop.Admin.Commands.DatasetQualityInformation;

public class UpdateCommand : IRequest
{
    public UpdateCommand(DatasetQualityInformationData qualityInformations)
        => DatasetQualityInformationData = qualityInformations;

    public DatasetQualityInformationData DatasetQualityInformationData { get; }
}