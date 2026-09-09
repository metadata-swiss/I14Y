using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PeriodOfTimeMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.PeriodOfTime, PeriodOfTimeModel>()
            .Map(dest => dest.End, src => src.End)
            .Map(dest => dest.Start, src => src.Start);

        config.NewConfig<PeriodOfTimeModel, Models.PeriodOfTime>()
            .Map(dest => dest.Start, src => src.Start)
            .Map(dest => dest.End, src => src.End);
    }
}