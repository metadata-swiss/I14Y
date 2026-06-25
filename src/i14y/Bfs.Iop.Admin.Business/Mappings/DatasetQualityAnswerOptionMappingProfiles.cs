using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityAnswerOptionMappingProfiles : Profile
{
    public DatasetQualityAnswerOptionMappingProfiles()
        : base(nameof(DatasetQualityAnswerOptionMappingProfiles))
    {
        CreateMap<DatasetQualityAnswerOptionModel, Models.DatasetQualityAnswerOption>()
            .ForMember(d => d.DetailMandatory, opt => opt.MapFrom(s => s.IsDetailMandatory));
            ;

        CreateMap<Models.DatasetQualityAnswerOption, DatasetQualityAnswerOptionModel>()
            .ForMember(d => d.IsDetailMandatory, opt => opt.MapFrom(s => s.DetailMandatory));
            ;
    }
}