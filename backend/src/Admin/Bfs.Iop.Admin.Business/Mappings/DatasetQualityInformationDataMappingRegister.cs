using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationDataMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DatasetQualityInformationDataModel, Models.DatasetQualityInformationData>();

        config.NewConfig<Models.DatasetQualityInformationData, DatasetQualityInformationDataModel>();
    }
}