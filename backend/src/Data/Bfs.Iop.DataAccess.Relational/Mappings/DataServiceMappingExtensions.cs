using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class DataServiceMappingExtensions
{
    public static DataServiceModel MapToDataServiceModel(
        this DataService entity,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var rightsStatementsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var licenseTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<LicenseTypesVocabulary>();
        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new()
        {
            AccessRights = entity.AccessRights.MapToVocabularyEntryModel(rightsStatementsVocabulary),
            ConformsTo = entity.ConformsTo.Select(x => x.MapToResourceModel()).ToList(),
            ContactPoints = entity.ContactPoint.Select(x => x.MapToVCardModel()).ToList(),
            Description = entity.Description.MapToMultiLanguageModel(),
            Documentation = entity.Documentation.Select(x => x.MapToResourceModel()).ToList(),
            EndpointDescriptions = entity.EndpointDescription.Select(x => x.MapToResourceModel()).ToList(),
            EndpointUrls = entity.EndpointUrl.Select(x => x.MapToResourceModel()).ToList(),
            Id = entity.Id,
            Identifiers = entity.Identifiers,
            Issued = entity.Issued,
            Keywords = entity.Keyword.Select(x => x.MapToKeywordModel()).ToList(),
            LandingPages = entity.LandingPage.Select(x => x.MapToResourceModel()).ToList(),
            License = entity.License?.MapToVocabularyEntryModel(licenseTypesVocabulary),
            Modified = entity.Modified,
            PreviousVersion = entity.PreviousVersionId.HasValue 
                ? new IdModel() { Id = entity.PreviousVersionId.Value } 
                : null,
            PublicationLevel = entity.PublicationLevel,
            PublicationLevelProposal = entity.PublicationLevelProposal,
            Publisher = entity.Publisher.MapToAgentModel(vocabulariesService),
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationStatusProposal = entity.RegistrationStatusProposal,
            ResponsibleDeputy = entity.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = entity.ResponsiblePerson?.MapToIopPersonModel(),
            ServesDatasets = entity.Datasets.Select(x => x.MapToIdModel()).ToList(),
            System = entity.MapSystemInfoToSystemInfoModel(),
            Themes = entity.Theme.MapToVocabularyEntryModels(themesVocabulary).ToList(),
            Title = entity.Title.MapToMultiLanguageModel(),
            Version = entity.Version,
            VersionNotes = entity.VersionNotes?.MapToMultiLanguageModel()
        };
    }

    public static DataService MapToDataService(
        this DataServiceInputModel inputModel,
        Guid publisherId,
        Guid? responsiblePersonId,
        Guid? responsibleDeputyId,
        IIdentifierGenerator identifierGenerator,
        DataService? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.AccessRights = inputModel.AccessRights.Code;
        entity.ConformsTo = inputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.ContactPoint = inputModel.ContactPoints.MapToVCards(entity.ContactPoint).ToList();
        entity.Datasets = inputModel.ServesDatasets.MapToDataServiceDataset(entity.Datasets).ToList();
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.Documentation = inputModel.Documentation.MapToResources(entity.Documentation).ToList();
        entity.EndpointDescription = inputModel.EndpointDescriptions.MapToResources(entity.EndpointDescription).ToList();
        entity.EndpointUrl = inputModel.EndpointUrls.MapToResources(entity.EndpointUrl).ToList();
        entity.Identifiers = inputModel.Identifiers.Any()
            ? inputModel.Identifiers.ToArray()
            : [identifierGenerator.GenerateIdentifier<DataService>(inputModel.Title)]; // Set the title as identifier, if none is provided
        entity.Issued = inputModel.Issued;
        entity.Keyword = inputModel.Keywords.MapToKeywords(entity.Keyword).ToList();
        entity.LandingPage = inputModel.LandingPages.MapToResources(entity.LandingPage).ToList();
        entity.License = inputModel.License?.Code;
        entity.Modified = inputModel.Modified;
        entity.PreviousVersionId = inputModel.PreviousVersion?.Id;
        entity.PublisherId = publisherId;
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.Theme = inputModel.Themes.Select(x => x.Code).ToArray();
        entity.Title = inputModel.Title.MapToMultiLanguage();
        entity.Version = inputModel.Version;
        entity.VersionNotes = inputModel.VersionNotes?.MapToMultiLanguage();

        return entity;
    }

    private static IdModel MapToIdModel(this DataServiceDataset entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Id = entity.DatasetId
        };
    }

    private static DataServiceDataset MapToDataServiceDataset(
        this IdModel idModel,
        DataServiceDataset? entity = null)
    {
        ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

        entity ??= new();

        entity.DatasetId = idModel.Id;

        return entity;
    }

    private static IEnumerable<DataServiceDataset> MapToDataServiceDataset(
        this IEnumerable<IdModel> inputModels,
        ICollection<DataServiceDataset> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToDataServiceDataset(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
