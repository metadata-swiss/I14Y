using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Services.Extensions;
using Bfs.Iop.Core.Vocabularies;

namespace Bfs.Iop.Core.Mappings;

internal static class SearchResultModelMappingExtensions
{
    public static SearchResultModel MapToSearchResultModel(
        this CatalogSearchResultEntry entry,
        AgentModel publisherModel,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));
        ArgumentNullException.ThrowIfNull(publisherModel, nameof(publisherModel));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var rightsStatementsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var businessEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>();
        var fileTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<FileTypesVocabulary>();
        var lifeEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>();
        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new SearchResultModel()
        {
            AccessRights = entry.AccessRights?.MapToVocabularyEntryModel(rightsStatementsVocabulary),
            BusinessEvents = entry.BusinessEvents.Select(x => x.MapToVocabularyEntryModel(businessEventsVocabulary)),
            ConceptType = entry.ConceptType,
            Description = entry.Description,
            Formats = entry.Formats.Select(x => x.MapToVocabularyEntryModel(fileTypesVocabulary)),
            Id = entry.Id,
            Identifier = entry.Identifier,
            LifeEvents = entry.LifeEvents.Select(x => x.MapToVocabularyEntryModel(lifeEventsVocabulary)),
            PublicationLevel = entry.PublicationLevel,
            PublicationLevelProposal = entry.PublicationLevelProposal,
            Publisher = publisherModel,
            RegistrationStatus = entry.RegistrationStatus,
            RegistrationStatusProposal = entry.RegistrationStatusProposal,
            Structure = entry.HasStructure.HasValue
                ? entry.HasStructure.Value
                    ? SearchStructureOption.WithStructure
                    : SearchStructureOption.WithoutStructure
                : null,
            System = MapToSystemInfoModel(entry),
            Themes = entry.Themes.Select(x => x.MapToVocabularyEntryModel(themesVocabulary)),
            Title = entry.Title,
            Type = entry.Type,
            ValidFrom = entry.ValidFrom,
            ValidTo = entry.ValidTo,
            Version = entry.Version,
        };
    }

    // Kept local rather than moved down with the other System mappers, because it maps a Lucene
    // search hit and Bfs.Iop.Core.Data must not know the engine exists. It leaves with this file
    // when Core stops answering search from an in-process index.
    private static SystemInfoModel MapToSystemInfoModel(CatalogSearchResultEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry, nameof(entry));

        return new()
        {
            CreatedAt = entry.CreatedAt,
            CreationType = entry.CreationType,
            ModifiedAt = entry.ModifiedAt,
        };
    }
}
