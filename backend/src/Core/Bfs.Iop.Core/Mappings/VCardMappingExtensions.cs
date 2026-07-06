using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;

namespace Bfs.Iop.Core.Mappings;

internal static class VCardMappingExtensions
{
    public static VCardModel MapToVCardModel(this VCard entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Fn = entity.Fn?.MapToMultiLanguageModel(),
            HasAddress = entity.AdrWork?.MapToMultiLanguageModel(),
            HasEmail = entity.EmailInternet ?? string.Empty,
            HasTelephone = entity.TelWorkVoice,
            Kind = Enum.Parse<VCardKind>(entity.Child),
            Note = entity.Note?.MapToMultiLanguageModel()
        };
    }

    public static VCard MapToVCard(this VCardModel inputModel, VCard? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.AdrWork = inputModel.HasAddress?.MapToMultiLanguage();
        entity.Child = inputModel.Kind.ToString();
        entity.EmailInternet = inputModel.HasEmail;
        entity.Fn = inputModel.Fn?.MapToMultiLanguage();
        entity.Note = inputModel.Note?.MapToMultiLanguage();
        entity.TelWorkVoice = inputModel.HasTelephone;

        return entity;
    }

    public static IEnumerable<VCard> MapToVCards(
       this IEnumerable<VCardModel> models,
       ICollection<VCard> entities)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return models.Select(
            (v, i) => v.MapToVCard(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
