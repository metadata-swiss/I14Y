using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class PeriodOfTimeMappingProfiles : Profile
{
    public PeriodOfTimeMappingProfiles()
        : base(nameof(PeriodOfTimeMappingProfiles))
    {
        CreateMap<Models.PeriodOfTime, PeriodOfTimeModel>()
            .ForMember(d => d.End, opt => opt.MapFrom(s => s.End))
            .ForMember(d => d.Start, opt => opt.MapFrom(s => s.Start));

        CreateMap<PeriodOfTimeModel, Models.PeriodOfTime>()
            .ForMember(d => d.Start, opt => opt.MapFrom(s => s.Start))
            .ForMember(d => d.End, opt => opt.MapFrom(s => s.End));
    }
}