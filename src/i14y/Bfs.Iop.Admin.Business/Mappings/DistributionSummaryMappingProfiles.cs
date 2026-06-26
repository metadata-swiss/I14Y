using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System.Linq;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DistributionSummaryMappingProfiles : Profile
{
    public DistributionSummaryMappingProfiles()
        : base(nameof(DistributionSummaryMappingProfiles))
    {
        CreateMap<DcatDistributionModel, DistributionSummary>()
            .ForMember(d => d.ByteSize, opt => opt.MapFrom(s => s.ByteSize))
            .ForMember(d => d.Format, opt => opt.MapFrom(s => s.Format))
            .ForMember(d => d.Id, opt => opt.MapFrom(s => s.Id))
            .ForMember(d => d.Published, opt => opt.MapFrom(s => s.Issued))
            .ForMember(d => d.Languages, opt => opt.MapFrom(s => s.Languages.Select(x => x.Code)))
            .ForMember(d => d.LastUpdated, opt => opt.MapFrom(s => s.Modified))
            .ForMember(d => d.Title, opt => opt.MapFrom(s => s.Title));
    }
}