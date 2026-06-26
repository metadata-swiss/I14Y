using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationDataMappingProfiles : Profile
{
    public DatasetQualityInformationDataMappingProfiles()
        : base(nameof(DatasetQualityAnswerOptionMappingProfiles))
    {
        CreateMap<DatasetQualityInformationDataModel, Models.DatasetQualityInformationData>()
            ;

        CreateMap<Models.DatasetQualityInformationData, DatasetQualityInformationDataModel>()
            ;
    }
}