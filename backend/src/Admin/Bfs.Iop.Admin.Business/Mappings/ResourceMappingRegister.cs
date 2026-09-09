using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class ResourceMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Resource, ResourceModel>()
            .Map(d => d.Label, s => s.Label)
            .Map(d => d.Uri, s => s.Href);

        config.NewConfig<ResourceModel, Resource>()
            .Map(d => d.Href, s => s.Uri)
            .Map(d => d.Label, s => s.Label);
    }
}