using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class QualifiedAttributionMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<QualifiedAttribution, DcatQualifiedAttributionInputModel>()
            .Map(d => d.HadRole, s => s.HadRole)
            .Map(d => d.Agent, s => s.Agent);

        config.NewConfig<DcatQualifiedAttributionModel, QualifiedAttribution>()
            .Map(d => d.HadRole, s => s.HadRole)
            .Map(d => d.Agent, s => s.Agent)
            .Ignore(d => d.Id)
            .Ignore(d => d.DatasetQualifiedAttributionId);
    }
}