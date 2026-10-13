using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Lucene.Search;
using Bfs.Iop.Core.Lucene;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Vocabularies;

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
        var themes = vocabulariesService.GetAllThemes();

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
            System = entry.MapToSystemInfoModel(),
            Themes = entry.Themes.Select(key => themes.FirstOrDefault(x => ThemeSearchKey.For(x) == key) ?? UnresolvedTheme(key)),
            Title = entry.Title,
            Type = entry.Type,
            ValidFrom = entry.ValidFrom,
            ValidTo = entry.ValidTo,
            Version = entry.Version,
        };
    }

    // A key no registered taxonomy offers any more: keep what it says rather than dropping it.
    private static VocabularyEntryModel UnresolvedTheme(string key)
    {
        var parts = ThemeSearchKey.TrySplit(key);

        return new VocabularyEntryModel
        {
            Code = parts?.Code ?? key,
            VocabularyIdentifier = parts?.VocabularyIdentifier ?? string.Empty
        };
    }

    private static SystemInfoModel MapToSystemInfoModel(this CatalogSearchResultEntry entry)
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