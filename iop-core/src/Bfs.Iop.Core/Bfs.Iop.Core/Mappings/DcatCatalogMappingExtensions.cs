using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Services.Contracts;

namespace Bfs.Iop.Core.Mappings;

internal static class DcatCatalogMappingExtensions
{
    public static DcatCatalogModel MapToDcatCatalogModel(this DcatCatalog entity, IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        return new()
        {
            Description = entity.Description.MapToMultiLanguageModel(),
            Id = entity.Id,
            Publisher = entity.Publisher.MapToAgentModel(vocabulariesService),
            System = entity.MapSystemInfoToSystemInfoModel(),
            Title = entity.Title.MapToMultiLanguageModel(),
            ThemeTaxonomy = entity.ThemeTaxonomy ?? []
        };
    }

    public static DcatCatalog MapToDcatCatalog(this DcatCatalogInputModel model, Guid publisherId, DcatCatalog? entity = null)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));

        entity ??= new();

        entity.Description = model.Description.MapToMultiLanguage();
        entity.PublisherId = publisherId;
        entity.ThemeTaxonomy = model.ThemeTaxonomy.ToArray();
        entity.Title = model.Title.MapToMultiLanguage();

        return entity;
    }
}
