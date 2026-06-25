using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings
{
    internal class DatasetQualityQuestionMappingProfiles : Profile
    {
        public DatasetQualityQuestionMappingProfiles()
            : base(nameof(DatasetQualityQuestionMappingProfiles))
        {
            CreateMap<DatasetQualityQuestionModel, Models.DatasetQualityQuestion>()
                .ForMember(d => d.Mandatory, opt => opt.MapFrom(s => s.IsMandatory))
                ;

            CreateMap<Models.DatasetQualityQuestion, DatasetQualityQuestionModel>()
                .ForMember(d => d.IsMandatory, opt => opt.MapFrom(s => s.Mandatory))
                ;
        }
    }
}