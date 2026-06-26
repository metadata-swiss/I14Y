using AutoMapper;
using Bfs.Iop.Admin.Models;
using Bfs.Iop.Core.Abstractions.Models;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DcatCatalogRecordsMappingProfile : Profile
{
    public DcatCatalogRecordsMappingProfile()
    {
        CreateMap<DcatCatalogModel, DcatCatalog>();

        CreateMap<DcatCatalogRecordInput, DcatCatalogRecordInputModel>();

        CreateMap<DcatCatalogRecordModel, DcatCatalogRecordInput>()
            .ForMember(d => d.CatalogId, opt => opt.MapFrom(s => s.DcatCatalogId))
            .ForMember(d => d.CatalogTitle, opt => opt.Ignore());

        CreateMap<DcatCatalogResource, DcatCatalogResourceModel>()
            .ForMember(d => d.ResourceType, opt => opt.MapFrom(s => Enum.Parse<DcatCatalogType>(s.ResourceType)));

        CreateMap<DcatCatalogResourceModel, DcatCatalogResource>()
            .ForMember(d => d.ResourceType, opt => opt.MapFrom(s => s.ResourceType.ToString()));

        CreateMap<DcatVocabularyEntry, DcatCatalogThemeInputModel>();

        CreateMap<DcatCatalogThemeModel, DcatVocabularyEntry>();
    }
}