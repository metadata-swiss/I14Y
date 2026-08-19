using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Services.Contracts;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Services.Extensions;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Mappings;

/// <summary>
/// Turns a raw index hit into the public <see cref="SearchResultModel"/>.
/// <para>
/// This mirrors the mapping IOP Core applies to its own Lucene hits. It is duplicated rather than
/// shared because the two engines own separate result types, so the vocabularies resolved here must
/// stay in step with Core's <c>SearchResultModelMappingExtensions</c> or the two engines will return
/// different labels for the same document.
/// </para>
/// </summary>
internal static class SearchResultMappingExtensions
{
    public static SearchResultModel MapToSearchResultModel(
        this CatalogSearchResultEntry entry,
        AgentModel publisherModel,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(publisherModel);
        ArgumentNullException.ThrowIfNull(vocabulariesService);

        var rightsStatementsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var businessEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>();
        var fileTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<FileTypesVocabulary>();
        var lifeEventsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>();
        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new SearchResultModel
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
            System = new SystemInfoModel
            {
                CreatedAt = entry.CreatedAt,
                CreationType = entry.CreationType,
                ModifiedAt = entry.ModifiedAt,
            },
            Themes = entry.Themes.Select(x => x.MapToVocabularyEntryModel(themesVocabulary)),
            Title = entry.Title,
            Type = entry.Type,
            ValidFrom = entry.ValidFrom,
            ValidTo = entry.ValidTo,
            Version = entry.Version,
        };
    }
}
