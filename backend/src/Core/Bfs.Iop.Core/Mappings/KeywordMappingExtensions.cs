using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Mappings;

internal static class KeywordMappingExtensions
{
    public static void MapToKeyword(this KeywordModel model, Keyword keyword)
    {
        ArgumentNullException.ThrowIfNull(model, nameof(model));
        ArgumentNullException.ThrowIfNull(keyword, nameof(keyword));

        keyword.Text = model.Label?.MapToMultiLanguage()!;
        keyword.Uri = model.Uri;
    }

    public static IEnumerable<Keyword> MapToKeywords(
        this IEnumerable<KeywordModel> models,
        ICollection<Keyword> entities)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return models.Select((m, i) =>
        {
            var keyword = entities.Count > i
                ? entities.ElementAt(i)
                : new();

            m.MapToKeyword(keyword);
            return keyword;
        });
    }

    public static KeywordModel MapToKeywordModel(this Keyword entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Label = entity.Text?.MapToMultiLanguageModel(),
            Uri = entity.Uri
        };
    }
}
