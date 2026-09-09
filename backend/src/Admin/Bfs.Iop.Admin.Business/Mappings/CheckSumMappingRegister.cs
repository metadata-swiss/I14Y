using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CheckSumMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ChecksumModel, CheckSum>()
            .TwoWays();

        config.NewConfig<CheckSum, ChecksumInputModel>()
            .Map(dest => dest.Algorithm, src => new CodeInputModel
            {
                Code = src.Algorithm.Code
            })
            .Map(dest => dest.ChecksumValue, src => src.ChecksumValue);
    }
}