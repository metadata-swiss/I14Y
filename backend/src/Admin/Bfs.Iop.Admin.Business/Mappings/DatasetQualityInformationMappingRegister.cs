using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DatasetQualityInformationModel, DatasetQualityInformation>();

        config.NewConfig<DatasetQualityInformation, DatasetQualityInformationModel>();
    }
}