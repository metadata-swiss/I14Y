using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Services.Extensions;
using Bfs.Iop.Core.Tools;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Mappings;

internal static class PublicServiceMappingExtensions
{
    public static PublicServiceModel MapToPublicServiceModel(
        this PublicService entity,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var businessEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>();
        var iso639LanguagesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<Iso639LanguagesVocabulary>();
        var lifeEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>();
        var spatialCHVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<SpatialCHVocabulary>();
        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new()
        {
            BusinessEvents = entity.BusinessEvents.MapToVocabularyEntryModels(businessEventsVocabulary),
            Channels = entity.Channels.Select(x => x.MapToChannelModel(vocabulariesService)),
            Description = entity.Description.MapToMultiLanguageModel(),
            Id = entity.Id,
            Identifiers = entity.Identifiers,
            IsDescribedAt = entity.IsDescribedAt.Select(x => x.MapToIdModel()),
            Keywords = entity.Keyword.Select(x => x.MapToKeywordModel()),
            Languages = entity.Language.MapToVocabularyEntryModels(iso639LanguagesVocabulary),
            LifeEvents = entity.LifeEvents.MapToVocabularyEntryModels(lifeEventsVocabulary),
            Name = entity.Title.MapToMultiLanguageModel(),
            PublicationLevel = entity.PublicationLevel,
            PublicationLevelProposal = entity.PublicationLevelProposal,
            Publisher = entity.Publisher.MapToAgentModel(vocabulariesService),
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationStatusProposal = entity.RegistrationStatusProposal,
            Relations = entity.Relation.Select(x => x.MapToIdModel()),
            Requires = entity.Requires.Select(x => x.MapToIdModel()),
            ResponsibleDeputy = entity.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = entity.ResponsiblePerson?.MapToIopPersonModel(),
            System = entity.MapSystemInfoToSystemInfoModel(),
            Sectors = entity.Sector.MapToVocabularyEntryModels(themesVocabulary),
            Spatial = entity.Spatial.AsEnumerable(),
            SpatialCH = entity.SpatialCH.MapToVocabularyEntryModels(spatialCHVocabulary),
            ThematicAreas = entity.ThematicArea.MapToVocabularyEntryModels(themesVocabulary),
        };
    }

    public static PublicService MapToPublicService(
        this PublicServiceInputModel inputModel,
        IReadOnlyDictionary<string, Guid> agentsMappingTable,
        Guid publisherId,
        Guid? responsiblePersonId,
        Guid? responsibleDeputyId,
        IIdentifierGenerator identifierGenerator,
        PublicService? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));
        ArgumentNullException.ThrowIfNull(agentsMappingTable, nameof(agentsMappingTable));

        entity ??= new();

        entity.BusinessEvents = inputModel.BusinessEvents.Select(x => x.Code).ToArray();
        entity.Channels = inputModel.Channels.MapToChannels(entity.Channels, agentsMappingTable).ToList();
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.Identifiers = inputModel.Identifiers.Any()
            ? [.. inputModel.Identifiers]
            : [identifierGenerator.GenerateIdentifier<PublicService>(inputModel.Name)];
        entity.IsDescribedAt = inputModel.IsDescribedAt.MapToPublicServiceIsDescribedAtEntities(entity.IsDescribedAt).ToList();
        entity.Keyword = inputModel.Keywords.MapToKeywords(entity.Keyword).ToList();
        entity.Language = inputModel.Languages.Select(x => x.Code).ToArray();
        entity.LifeEvents = inputModel.LifeEvents.Select(x => x.Code).ToArray();
        entity.Title = inputModel.Name.MapToMultiLanguage();
        entity.PublisherId = publisherId;
        entity.Relation = inputModel.Relations.MapToPublicServiceRelations(entity.Relation).ToList();
        entity.Requires = inputModel.Requires.MapToPublicServiceRequiresEntities(entity.Requires).ToList();
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.Sector = inputModel.Sectors.Select(x => x.Code).ToArray();
        entity.Spatial = inputModel.Spatial.ToArray();
        entity.ThematicArea = inputModel.ThematicAreas.Select(x => x.Code).ToArray();
        entity.SpatialCH = inputModel.SpatialCH.Select(x => x.Code).ToArray();

        return entity;
    }

    private static IdModel MapToIdModel(this PublicServiceIsDescribedAt entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Id = entity.IsDescribedAtId,
        };
    }

    private static IdModel MapToIdModel(this PublicServiceRelation entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Id = entity.RelationId,
        };
    }

    private static IdModel MapToIdModel(this PublicServiceRequires entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Id = entity.RequiresId,
        };
    }

    private static PublicServiceIsDescribedAt MapToPublicServiceIsDescribedAt(
        this IdModel idModel,
        PublicServiceIsDescribedAt? entity = null)
        {
            ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

            entity ??= new();

            entity.IsDescribedAtId = idModel.Id;

            return entity;
        }

    private static IEnumerable<PublicServiceIsDescribedAt> MapToPublicServiceIsDescribedAtEntities(
        this IEnumerable<IdModel> inputModels,
        ICollection<PublicServiceIsDescribedAt> entities)
        {
            ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
            ArgumentNullException.ThrowIfNull(entities, nameof(entities));

            return inputModels.Select(
                (r, i) => r.MapToPublicServiceIsDescribedAt(entities.Count > i
                    ? entities.ElementAt(i)
                    : new()));
        }

    private static PublicServiceRelation MapToPublicServiceRelation(
        this IdModel idModel,
        PublicServiceRelation? entity = null)
        {
            ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

            entity ??= new();

            entity.RelationId = idModel.Id;

            return entity;
        }

    private static IEnumerable<PublicServiceRelation> MapToPublicServiceRelations(
        this IEnumerable<IdModel> inputModels,
        ICollection<PublicServiceRelation> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToPublicServiceRelation(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }

    private static PublicServiceRequires MapToPublicServiceRequires(
        this IdModel idModel,
        PublicServiceRequires? entity = null)
    {
        ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

        entity ??= new();

        entity.RequiresId = idModel.Id;

        return entity;
    }

    private static IEnumerable<PublicServiceRequires> MapToPublicServiceRequiresEntities(
        this IEnumerable<IdModel> inputModels,
        ICollection<PublicServiceRequires> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToPublicServiceRequires(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
