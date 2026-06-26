using AutoMapper;
using Bfs.Iop.Core.Abstractions.Models;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DatasetQualityInformationLinkMappingProfiles : Profile
{
    public DatasetQualityInformationLinkMappingProfiles()
        : base(nameof(DatasetQualityInformationLinkMappingProfiles))
    {
        CreateMap<DatasetQualityInformationLinkModel, Models.DatasetQualityInformationLink>()
            ;

        CreateMap<Models.DatasetQualityInformationLink, DatasetQualityInformationLinkModel>()
            ;
    }
}