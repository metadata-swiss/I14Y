using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class MappingTableExportModelMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<MappingTableModel, MappingTableExportModel>()
            .Map(dest => dest.Relations, _ => Enumerable.Empty<MappingRelationModel>());
    }
}

