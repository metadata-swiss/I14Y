using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class ChannelMappingExtensions
{
    public static ChannelModel MapToChannelModel(this Channel entity, IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var typesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ChannelTypesVocabulary>();

        return new()
        {
            Address = entity.Address?.MapToMultiLanguageModel(),
            Description = entity.Description?.MapToMultiLanguageModel(),
            Email = entity.Email,
            Fax = entity.Fax,
            Id = entity.Id,
            Identifier = entity.Identifier,
            Mobile = entity.Mobile,
            OpeningHours = entity.OpeningHours,
            OwnedBy = entity.OwnedBy.Select(x => x.OwnedBy.MapToAgentModel(vocabulariesService)).ToList(),
            Phone = entity.Phone,
            Type = entity.Type?.MapToVocabularyEntryModel(typesVocabulary),
            Url = entity.Url,
        };
    }

    public static Channel MapToChannel(
        this ChannelInputModel inputModel,
        IReadOnlyDictionary<string, Guid> agentsMappingTable,
        Channel? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));
        ArgumentNullException.ThrowIfNull(agentsMappingTable, nameof(agentsMappingTable));

        entity ??= new();

        entity.Address = inputModel.Address?.MapToMultiLanguage()!;
        entity.Description = inputModel.Description?.MapToMultiLanguage()!;
        entity.Email = inputModel.Email;
        entity.Fax = inputModel.Fax;
        entity.Identifier = inputModel.Identifier;
        entity.Mobile = inputModel.Mobile;
        entity.OpeningHours = inputModel.OpeningHours;
        entity.OwnedBy = inputModel.OwnedBy.Select(x => new ChannelOwnedBy() { OwnedById = agentsMappingTable[x.Identifier] }).ToList();
        entity.Phone = inputModel.Phone;
        entity.Type = inputModel.Type?.Code;
        entity.Url = inputModel.Url;

        return entity;
    }

    public static IEnumerable<Channel> MapToChannels( 
        this IEnumerable<ChannelInputModel> models,
        ICollection<Channel> entities,
        IReadOnlyDictionary<string, Guid> agentsMappingTable)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        ArgumentNullException.ThrowIfNull(agentsMappingTable, nameof(agentsMappingTable));

        return models.Select(
            (x, y) => x.MapToChannel(
                agentsMappingTable, 
                entities.Count > y 
                    ? entities.ElementAt(y)
                    : new()));
    }
}
