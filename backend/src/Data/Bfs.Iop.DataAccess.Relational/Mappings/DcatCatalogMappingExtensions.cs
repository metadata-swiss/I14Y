using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

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
