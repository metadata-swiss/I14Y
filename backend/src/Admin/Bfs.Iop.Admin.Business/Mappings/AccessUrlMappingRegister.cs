using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class AccessUrlMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Models.AccessUrl, ResourceModel>()
            .Map(dest => dest.Label, src => src.Label)
            .Map(dest => dest.Uri, src => src.Href);

        config.NewConfig<ResourceModel, Models.AccessUrl>()
            .Ignore(dest => dest.IsDownload)
            .Map(dest => dest.Href, src => src.Uri)
            .Map(dest => dest.Label, src => src.Label);
    }
}