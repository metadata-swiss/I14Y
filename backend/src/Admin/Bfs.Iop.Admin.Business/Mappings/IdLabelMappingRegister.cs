using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class IdLabelMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DataServiceModel, Models.IdLabel>()
            .Map(dest => dest.Id, src => src.Id)
            .Map(dest => dest.Label, src => src.Title);
    }
}