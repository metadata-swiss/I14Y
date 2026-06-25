using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class QualifiedAttributionProfile : Profile
{
    public QualifiedAttributionProfile() : base(nameof(QualifiedAttributionProfile))
    {
        CreateMap<QualifiedAttribution, DcatQualifiedAttributionInputModel>()
            .ForMember(d => d.HadRole, opt => opt.MapFrom(s => s.HadRole))
            .ForMember(d => d.Agent, opt => opt.MapFrom(s => s.Agent));

        CreateMap<DcatQualifiedAttributionModel, QualifiedAttribution>()
            .ForMember(d => d.HadRole, opt => opt.MapFrom(s => s.HadRole))
            .ForMember(d => d.Agent, opt => opt.MapFrom(s => s.Agent))
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.DatasetQualifiedAttributionId, opt => opt.Ignore());
    }
}