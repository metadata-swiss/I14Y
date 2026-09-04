using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityAnswerOptionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DatasetQualityAnswerOptionModel, Models.DatasetQualityAnswerOption>()
            .Map(dest => dest.DetailMandatory, src => src.IsDetailMandatory);

        config.NewConfig<Models.DatasetQualityAnswerOption, DatasetQualityAnswerOptionModel>()
            .Map(dest => dest.IsDetailMandatory, src => src.DetailMandatory);
    }
}