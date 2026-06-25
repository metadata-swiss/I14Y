using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class QualifiedRelationProfile : Profile
{
    public QualifiedRelationProfile() : base(nameof(QualifiedRelationProfile))
    {
        CreateMap<QualifiedRelation, DcatQualifiedRelationInputModel>()
            .ForMember(d => d.HadRole, opt => opt.MapFrom(s => s.HadRole))
            .ForMember(d => d.Relation, opt => opt.MapFrom(s => s.Relation));

        CreateMap<DcatQualifiedRelationModel, QualifiedRelation>()
            .ForMember(d => d.DatasetQualifiedRelationId, opt => opt.Ignore())
            .ForMember(d => d.HadRole, opt => opt.MapFrom(s => s.HadRole))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Relation, opt => opt.MapFrom(s => s.Relation));
    }
}