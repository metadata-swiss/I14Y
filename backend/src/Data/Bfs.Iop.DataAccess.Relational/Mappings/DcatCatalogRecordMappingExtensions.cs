using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class DcatCatalogRecordMappingExtensions
{
    public static DcatCatalogRecordModel MapToDcatCatalogRecordModel(this DcatCatalogRecord entity, IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        return new DcatCatalogRecordModel()
        {
            DcatCatalogId = entity.DcatCatalogId,
            Id = entity.Id,
            PrimaryTopic = entity.PrimaryTopic.MapToDcatCatalogResourceModel(),
            Themes = entity.Themes?.Select(x => x.MapToDcatThemeModel(vocabulariesService)).ToList() ?? []
        };
    }

    public static DcatCatalogRecord MapToDcatCatalogRecord(
        this DcatCatalogRecordInputModel inputModel, 
        Guid dcatCatalogId, 
        DcatCatalogRecord? entity = null)
    {
        entity ??= new()
        {
            Id = Guid.NewGuid()
        };

        entity.DcatCatalogId = dcatCatalogId;
        entity.PrimaryTopic = inputModel.PrimaryTopic.MapToDcatCatalogResource();

        return entity;
    }

    private static DcatCatalogResourceModel MapToDcatCatalogResourceModel(this DcatCatalogResource entity) =>
        new()
        {
            ResourceId = entity.ResourceId,
            ResourceType = Enum.Parse<DcatCatalogType>(entity.ResourceType)
        };

    private static DcatCatalogResource MapToDcatCatalogResource(this DcatCatalogResourceModel model) => 
        new()
        {
            ResourceId = model.ResourceId,
            ResourceType = model.ResourceType.ToString()
        };

    private static DcatCatalogThemeModel MapToDcatThemeModel(this DcatCatalogTheme entity, IVocabulariesService vocabulariesService)
    {
        var vocabulary = vocabulariesService.GetExistingOrEmptyVocabulary(entity.ThemeTaxonomy);

        var entry = entity.Code.MapToVocabularyEntryModel(vocabulary);

        return new()
        {
            Code = entry.Code,
            Name = entry.Name,
            Uri = entry.Uri,
            ThemeTaxonomy = entity.ThemeTaxonomy
        };
    }

}
