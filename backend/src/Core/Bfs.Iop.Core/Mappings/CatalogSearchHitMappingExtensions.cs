using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Vocabularies;
using Bfs.Iop.IndexSearch.Contracts.Search;

namespace Bfs.Iop.Core.Mappings;

internal static class CatalogSearchHitMappingExtensions
{
    public static SearchResultModel MapToSearchResultModel(
        this CatalogSearchHit hit,
        AgentModel publisherModel,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(hit);
        ArgumentNullException.ThrowIfNull(publisherModel);
        ArgumentNullException.ThrowIfNull(vocabulariesService);

        var rightsStatements = vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var businessEvents = vocabulariesService.GetExistingOrEmptyVocabulary<BkBusinessEventsVocabulary>();
        var fileTypes = vocabulariesService.GetExistingOrEmptyVocabulary<FileTypesVocabulary>();
        var lifeEvents = vocabulariesService.GetExistingOrEmptyVocabulary<BkLifeEventsVocabulary>();
        var themes = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new SearchResultModel
        {
            AccessRights = hit.AccessRights?.MapToVocabularyEntryModel(rightsStatements),
            BusinessEvents = hit.BusinessEvents.Select(x => x.MapToVocabularyEntryModel(businessEvents)),
            ConceptType = hit.ConceptType,
            Description = hit.Description,
            Formats = hit.Formats.Select(x => x.MapToVocabularyEntryModel(fileTypes)),
            Id = hit.Id,
            Identifier = hit.Identifiers.FirstOrDefault(),
            LifeEvents = hit.LifeEvents.Select(x => x.MapToVocabularyEntryModel(lifeEvents)),
            PublicationLevel = hit.PublicationLevel,
            PublicationLevelProposal = hit.PublicationLevelProposal,
            Publisher = publisherModel,
            RegistrationStatus = hit.RegistrationStatus,
            RegistrationStatusProposal = hit.RegistrationStatusProposal,
            Structure = hit.HasStructure.HasValue
                ? hit.HasStructure.Value
                    ? SearchStructureOption.WithStructure
                    : SearchStructureOption.WithoutStructure
                : null,
            System = new SystemInfoModel
            {
                CreatedAt = hit.CreatedAt ?? default,
                CreationType = hit.CreationType,
                ModifiedAt = hit.ModifiedAt,
            },
            Themes = hit.Themes.Select(x => x.MapToVocabularyEntryModel(themes)),
            Title = hit.Title,
            Type = hit.Type,
            ValidFrom = hit.ValidFrom,
            ValidTo = hit.ValidTo,
            Version = hit.Version,
        };
    }
}
