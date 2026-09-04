using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityQuestionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DatasetQualityQuestionModel, DatasetQualityQuestion>()
            .Map(dest => dest.Mandatory, src => src.IsMandatory);

        config.NewConfig<DatasetQualityQuestion, DatasetQualityQuestionModel>()
            .Map(dest => dest.IsMandatory, src => src.Mandatory);
    }
}