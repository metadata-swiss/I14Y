using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Mappings;
using Bfs.Iop.Core.Vocabularies;
using Bfs.Iop.Search.Abstractions;

namespace Bfs.Iop.IndexSearch.Api.Mappings;

/// <summary>
/// Turns a raw index hit into the public <see cref="SearchResultModel"/>.
/// <para>
/// This is the single place a raw hit becomes a public model. IOP Core used to keep its own copy for
/// its in-process engine; that copy was deleted when Core became a gateway, precisely so the two
/// could not drift and return different labels for the same document. Do not reintroduce one on the
/// Core side — Core receives finished <see cref="SearchResultModel"/>s over the wire.
/// </para>
/// </summary>
internal static class SearchResultMappingExtensions
{
    public static SearchResultModel MapToSearchResultModel(
        this CatalogSearchResultEntry entry,
        AgentModel publisherModel,
        IVocabularyReader vocabularies)
    {
        ArgumentNullException.ThrowIfNull(entry);
        ArgumentNullException.ThrowIfNull(publisherModel);
        ArgumentNullException.ThrowIfNull(vocabularies);

        var rightsStatementsVocabulary = vocabularies.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var businessEventsVocabulary = vocabularies.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>();
        var fileTypesVocabulary = vocabularies.GetExistingOrEmptyVocabulary<FileTypesVocabulary>();
        var lifeEventsVocabulary = vocabularies.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>();
        var themesVocabulary = vocabularies.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

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
