using Bfs.Iop.Admin.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Mapster;
using System;

namespace Bfs.Iop.Admin.Business.Mappings;

internal class DcatCatalogRecordsMappingRegister : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DcatCatalogModel, DcatCatalog>();

        config.NewConfig<DcatCatalogRecordInput, DcatCatalogRecordInputModel>();

        config.NewConfig<DcatCatalogRecordModel, DcatCatalogRecordInput>()
            .Map(dest => dest.CatalogId, src => src.DcatCatalogId)
            .Ignore(dest => dest.CatalogTitle);

        config.NewConfig<DcatCatalogResource, DcatCatalogResourceModel>()
            .Map(dest => dest.ResourceType, src => Enum.Parse<DcatCatalogType>(src.ResourceType));

        config.NewConfig<DcatCatalogResourceModel, DcatCatalogResource>()
            .Map(dest => dest.ResourceType, src => src.ResourceType.ToString());

        config.NewConfig<DcatVocabularyEntry, DcatCatalogThemeInputModel>();

        config.NewConfig<DcatCatalogThemeModel, DcatVocabularyEntry>();
    }
}