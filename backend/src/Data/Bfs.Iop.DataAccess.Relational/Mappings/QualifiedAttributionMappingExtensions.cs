using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class QualifiedAttributionMappingExtensions
{
    public static DcatQualifiedAttributionModel MapToDcatQualifiedAttributionModel(
        this QualifiedAttribution entity,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var attributionRolesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<AttributionRolesVocabulary>();

        return new()
        {
            Agent = entity.Agent is not null
                ? entity.Agent.MapToIdentifierNameModel()
                : new IdentifierNameModel()
                {
                    Identifier = entity.ExternalAgentIdentifier,
                    Name = entity.ExternalAgentName?.MapToMultiLanguageModel()
                },
            HadRole = entity.HadRole.MapToVocabularyEntryModel(attributionRolesVocabulary),
        };
    }

    public static QualifiedAttribution MapToQualifiedAttribution(
        this DcatQualifiedAttributionInputModel inputModel,
        Guid? agentId,
        QualifiedAttribution? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.AgentId = agentId;
        entity.ExternalAgentIdentifier = agentId is null ? inputModel.Agent.Identifier : null;
        entity.ExternalAgentName = agentId is null ? inputModel.Agent.Name?.MapToMultiLanguage() : null;
        entity.HadRole = inputModel.HadRole.Code;

        return entity;
    }

    public static IEnumerable<QualifiedAttribution> MapToQualifiedAttributions(
        this IEnumerable<DcatQualifiedAttributionInputModel> models,
        ICollection<QualifiedAttribution> entities,
        IReadOnlyDictionary<string, Guid> agentsMappingTable)
    {
        ArgumentNullException.ThrowIfNull(models, nameof(models));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));
        ArgumentNullException.ThrowIfNull(agentsMappingTable, nameof(agentsMappingTable));

        return models.Select(
            (q, i) => q.MapToQualifiedAttribution(tryGetAgentId(q.Agent.Identifier),
                entities.Count > i
                    ? entities.ElementAt(i)
                    : new()));

        Guid? tryGetAgentId(string? agentIdentifier) =>
            agentIdentifier is not null && agentsMappingTable.TryGetValue(agentIdentifier, out var agentId)
                ? agentId
                : null;
    }
}
