using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Services.Extensions;

namespace Bfs.Iop.Core.Mappings;

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
            Themes = entity.Themes?.Select(x => x.MapToDcatThemeModel(vocabulariesService)) ?? []
        };
    }

    public static DcatCatalogRecord MapToDcatCatalogRecord(
        this DcatCatalogRecordInputModel inputModel, 
        Guid dcatCatalogId, 
        DcatCatalogRecord? entity = null)
    {
        entity ??= new();

        entity.DcatCatalogId = dcatCatalogId;
        entity.PrimaryTopic = inputModel.PrimaryTopic.MapToDcatCatalogResource();
        entity.Themes = inputModel.Themes.MapToDcatCatalogThemes(entity.Themes).ToList();

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

    public static DcatCatalogTheme MapToDcatCatalogTheme(
        this DcatCatalogThemeInputModel inputModel,
        DcatCatalogTheme? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.ThemeTaxonomy = inputModel.ThemeTaxonomy;
        entity.Code = inputModel.Code;

        return entity;
    }

    public static IEnumerable<DcatCatalogTheme> MapToDcatCatalogThemes(
        this IEnumerable<DcatCatalogThemeInputModel> models,
        ICollection<DcatCatalogTheme> entities)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return models.Select(
            (q, i) => q.MapToDcatCatalogTheme(
                entities.Count > i
                    ? entities.ElementAt(i)
                    : new()));
    }
}
