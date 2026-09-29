using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Mappings;
using Bfs.Iop.DataAccess.Vocabularies;
using Client = Bfs.Iop.IndexSearch.ApiClient;

namespace Bfs.Iop.Core.Mappings;

internal static class CatalogSearchHitMappingExtensions
{
    public static SearchResultModel MapToSearchResultModel(
        this Client.CatalogSearchHit hit,
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
            BusinessEvents = Codes(hit.BusinessEvents).Select(x => x.MapToVocabularyEntryModel(businessEvents)),
            ConceptType = IndexSearchEnumCrossing.Cross<Client.ConceptType, ConceptType>(hit.ConceptType),
            Description = Text(hit.Description),
            Formats = Codes(hit.Formats).Select(x => x.MapToVocabularyEntryModel(fileTypes)),
            Id = hit.Id,
            Identifier = Codes(hit.Identifiers).FirstOrDefault() ?? string.Empty,
            LifeEvents = Codes(hit.LifeEvents).Select(x => x.MapToVocabularyEntryModel(lifeEvents)),
            PublicationLevel = IndexSearchEnumCrossing.Cross<Client.PublicationLevel, PublicationLevel>(hit.PublicationLevel)
                ?? PublicationLevel.Internal,
            PublicationLevelProposal = IndexSearchEnumCrossing.Cross<Client.PublicationLevel, PublicationLevel>(hit.PublicationLevelProposal),
            Publisher = publisherModel,
            RegistrationStatus = IndexSearchEnumCrossing.Cross<Client.RegistrationStatus, RegistrationStatus>(hit.RegistrationStatus)
                ?? RegistrationStatus.Incomplete,
            RegistrationStatusProposal =
                IndexSearchEnumCrossing.Cross<Client.RegistrationStatus, RegistrationStatus>(hit.RegistrationStatusProposal),
            Structure = hit.HasStructure.HasValue
                ? hit.HasStructure.Value
                    ? SearchStructureOption.WithStructure
                    : SearchStructureOption.WithoutStructure
                : null,
            System = new SystemInfoModel
            {
                CreatedAt = hit.CreatedAt ?? default,
                CreationType = IndexSearchEnumCrossing.Cross<Client.CreationType, CreationType>(hit.CreationType),
                ModifiedAt = hit.ModifiedAt,
            },
            Themes = Codes(hit.Themes).Select(x => x.MapToVocabularyEntryModel(themes)),
            Title = Text(hit.Title),
            Type = IndexSearchEnumCrossing.Cross<Client.SearchResourceType, SearchResourceType>(hit.Type)!.Value,
            ValidFrom = hit.ValidFrom,
            ValidTo = hit.ValidTo,
            Version = hit.Version,
        };
    }

    private static IEnumerable<string> Codes(ICollection<string>? values) => values ?? [];

    private static MultiLanguageModel Text(Client.MultiLanguageModel? value) => value is null
        ? new MultiLanguageModel()
        : new MultiLanguageModel
        {
            De = value.De,
            En = value.En,
            Fr = value.Fr,
            It = value.It,
            Rm = value.Rm,
        };
}
