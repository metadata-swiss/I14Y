using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationLinkMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DatasetQualityInformationLinkModel, Models.DatasetQualityInformationLink>();

        config.NewConfig<Models.DatasetQualityInformationLink, DatasetQualityInformationLinkModel>();
    }
}