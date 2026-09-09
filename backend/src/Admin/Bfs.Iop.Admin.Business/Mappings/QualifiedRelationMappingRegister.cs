using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class QualifiedRelationMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<QualifiedRelation, DcatQualifiedRelationInputModel>()
            .Map(d => d.HadRole, s => s.HadRole)
            .Map(d => d.Relation, s => s.Relation);

        config.NewConfig<DcatQualifiedRelationModel, QualifiedRelation>()
            .Ignore(d => d.DatasetQualifiedRelationId)
            .Map(d => d.HadRole, s => s.HadRole)
            .Ignore(d => d.Id)
            .Map(d => d.Relation, s => s.Relation);
    }
}