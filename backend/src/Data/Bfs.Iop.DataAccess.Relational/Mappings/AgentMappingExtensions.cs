using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class AgentMappingExtensions
{
    public static AgentModel MapToAgentModel(this Agent agent, IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(agent, nameof(agent));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var spatialCHVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<SpatialCHVocabulary>();
        var classificationVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<LegalFormVocabulary>();

        return new()
        {
            Classification = agent.Classification?.MapToVocabularyEntryModel(classificationVocabulary),
            ContactPoint = agent.ContactPoint?.MapToVCardModel(),
            Description = agent.Description?.MapToMultiLanguageModel(),
            HomePage = agent.HomePage,
            Id = agent.Id,
            Identifier = agent.Identifier,
            Images = agent.Images.Select(x => x.MapToResourceModel()).ToList(),
            Name = agent.Name.MapToMultiLanguageModel(),
            PrefLabel = agent.PrefLabel.MapToMultiLanguageModel(),
            System = agent.MapSystemInfoToSystemInfoModel(),
            Spatial = agent.Spatial,
            SpatialCH = agent.SpatialCH.Select(x => x.MapToVocabularyEntryModel(spatialCHVocabulary)).ToList(),
            SubAgents = agent.SubAgents.Select(x => x.MapToIdNameModel()).ToList(),
            Uid = agent.Uid,
        };
    }

    public static Agent MapToAgent(this AgentInputModel agent, Agent? entity = null)
    {
        ArgumentNullException.ThrowIfNull(agent, nameof(agent));

        entity ??= new();

        entity.Classification = agent.Classification?.Code;
        entity.ContactPoint = agent.ContactPoint?.MapToVCard();
        entity.Description = agent.Description?.MapToMultiLanguage();
        entity.HomePage = agent.HomePage;
        entity.Identifier = agent.Identifier;
        entity.Images = [.. agent.Images.MapToResources(entity.Images)];
        entity.Name = agent.Name.MapToMultiLanguage();
        entity.PrefLabel = agent.PrefLabel.MapToMultiLanguage();
        entity.Spatial = [.. agent.Spatial];
        entity.SpatialCH = [.. agent.SpatialCH.Select(x => x.Code)];
        entity.SubAgents = [.. agent.SubAgents.MapToAgentSubAgentRelations(entity.SubAgents)];
        entity.Uid = agent.Uid;

        return entity;
    }

    private static IdNameModel MapToIdNameModel(this AgentSubAgentRelation relation)
    {
        ArgumentNullException.ThrowIfNull(relation, nameof(relation));

        return new IdNameModel()
        {
            Id = relation.SubAgentId,
            Name = relation.SubAgent?.Name.MapToMultiLanguageModel()
        };
    }

    private static AgentSubAgentRelation MapToAgentSubAgentRelation(
        this IdModel idModel,
        AgentSubAgentRelation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

        entity ??= new();

        entity.SubAgentId = idModel.Id;

        return entity;
    }

    private static IEnumerable<AgentSubAgentRelation> MapToAgentSubAgentRelations(
        this IEnumerable<IdModel> inputModels,
        ICollection<AgentSubAgentRelation> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToAgentSubAgentRelation(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
