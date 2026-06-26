using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class CheckSumMappingProfile : Profile
{
    public CheckSumMappingProfile()
        : base(nameof(CheckSumMappingProfile))
    {
        CreateMap<ChecksumModel, Models.CheckSum>().ReverseMap();

        CreateMap<CheckSum, ChecksumInputModel>()
            .ForMember(d => d.Algorithm, opt => opt.MapFrom(s => new CodeInputModel() { Code = s.Algorithm.Code }))
            .ForMember(d => d.ChecksumValue, opt => opt.MapFrom(s => s.ChecksumValue));
    }
}