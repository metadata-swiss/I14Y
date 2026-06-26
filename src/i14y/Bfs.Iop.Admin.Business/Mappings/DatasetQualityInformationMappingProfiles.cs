using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationMappingProfiles : Profile
{
    public DatasetQualityInformationMappingProfiles()
        : base(nameof(DatasetQualityInformationMappingProfiles))
    {
        CreateMap<DatasetQualityInformationModel, Models.DatasetQualityInformation>()
            ;
        CreateMap<Models.DatasetQualityInformation, DatasetQualityInformationModel>()
            ;
    }
}