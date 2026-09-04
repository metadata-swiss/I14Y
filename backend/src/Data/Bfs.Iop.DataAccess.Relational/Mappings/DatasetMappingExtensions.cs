using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class DatasetMappingExtensions
{
    public static DcatDatasetModel MapToDcatDatasetModel(
        this Dataset entity, 
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var rightsStatementsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<RightsStatementsVocabulary>();
        var confidentialityPersonsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ConfidentialityPersonsVocabulary>();
        var frequencyTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<FrequencyTypesVocabulary>();
        var geoIvIdsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<GeoIvIdsVocabulary>();
        var iso639LanguagesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<Iso639LanguagesVocabulary>();
        var relationshipRolesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<RelationshipRolesVocabulary>();
        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new()
        {
            AccessRights = entity.AccessRights.MapToVocabularyEntryModel(rightsStatementsVocabulary),
            ConfidentialityPerson = entity.ConfidentialityPerson?.MapToVocabularyEntryModel(confidentialityPersonsVocabulary),
            ConformsTo = entity.ConformsTo.Select(x => x.MapToResourceModel()).ToList(),
            ContactPoints = entity.ContactPoint.Select(x => x.MapToVCardModel()).ToList(),
            DataOwner = entity.DataOwner,
            Description = entity.Description.MapToMultiLanguageModel(),
            Distributions = entity.Distributions.Select(x => x.MapToDcatDistributionModel(vocabulariesService)).ToList(),
            Documentation = entity.Documentation.Select(x => x.MapToResourceModel()).ToList(),
            Frequency = entity.Frequency?.MapToVocabularyEntryModel(frequencyTypesVocabulary),
            GeoIvIds = entity.GeoIvId.MapToVocabularyEntryModels(geoIvIdsVocabulary).ToList(),
            Id = entity.Id,
            Identifiers = entity.Identifier,
            Images = entity.Image.Select(x => x.MapToResourceModel()).ToList(),
            IsReferencedBy = entity.IsReferencedBy.Select(x => x.MapToResourceModel()).ToList(),
            Issued = entity.Issued,
            Keywords = entity.Keyword.Select(x => x.MapToKeywordModel()).ToList(),
            LandingPages = entity.LandingPage.Select(x => x.MapToResourceModel()).ToList(),
            Languages = entity.Language.MapToVocabularyEntryModels(iso639LanguagesVocabulary).ToList(),
            Modified = entity.Modified,
            PreviousVersion = entity.PreviousVersionId.HasValue
                ? new() { Id = entity.PreviousVersionId!.Value }
                : default,
            ProcessId = entity.ProcessId,
            PublicationLevel = entity.PublicationLevel,
            PublicationLevelProposal = entity.PublicationLevelProposal,
            Publisher = entity.Publisher.MapToAgentModel(vocabulariesService),
            QualifiedAttributionComplement = entity.QualifiedAttributionComplement?.MapToMultiLanguageModel(),
            QualifiedAttributions = entity.QualifiedAttribution.Select(x => x.MapToDcatQualifiedAttributionModel(vocabulariesService)).ToList(),
            QualifiedRelations = entity.QualifiedRelation.Select(x => x.MapToQualifiedRelationModel(relationshipRolesVocabulary)).ToList(),
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationStatusProposal = entity.RegistrationStatusProposal,
            Relations = entity.Relation.Select(x => x.MapToResourceModel()).ToList(),
            ResponsibleDeputy = entity.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = entity.ResponsiblePerson?.MapToIopPersonModel(),
            RetentionPeriod = entity.RetentionPeriod,
            RetentionPeriodComplement = entity.RetentionPeriodDescription?.MapToMultiLanguageModel(),
            System = entity.MapSystemInfoToSystemInfoModel(), 
            Spatial = entity.Spatial ?? [],
            TemporalCoverage = entity.TemporalCoverage.Select(x => x.MapToDateOnlyPeriodOfTimeModel()).ToList(),
            Themes =  entity.Theme.MapToVocabularyEntryModels(themesVocabulary).ToList(),
            Title = entity.Title.MapToMultiLanguageModel(),
            Version = entity.Version,
            VersionNotes = entity.VersionNotes?.MapToMultiLanguageModel()
        };
    }

    public static Dataset MapToDataset(
        this DcatDatasetInputModel inputModel,
        Guid publisherId,
        Guid? responsiblePersonId,
        Guid? responsibleDeputyId,
        IReadOnlyDictionary<string, Guid> qualifiedAttributionsAgentsMappingTable,
        IIdentifierGenerator identifierGenerator,
        Dataset? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.AccessRights = inputModel.AccessRights.Code;
        entity.ConfidentialityPerson = inputModel.ConfidentialityPerson?.Code;
        entity.ConformsTo = inputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.ContactPoint = inputModel.ContactPoints.MapToVCards(entity.ContactPoint).ToList();
        entity.DataOwner = inputModel.DataOwner;
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.Distributions = inputModel.Distributions.MapToDistributions(entity.Distributions).ToList();
        entity.Documentation = inputModel.Documentation.MapToResources(entity.Documentation).ToList();
        entity.Frequency = inputModel.Frequency?.Code;
        entity.GeoIvId = inputModel.GeoIvIds.Select(x => x.Code).ToArray();
        entity.Identifier = inputModel.Identifiers.Any()
            ? inputModel.Identifiers.ToArray()
            : [identifierGenerator.GenerateIdentifier<Dataset>(inputModel.Title)]; // Set the title as identifier, if none is provided
        entity.Image = inputModel.Images.MapToResources(entity.Image).ToList();
        entity.IsReferencedBy = inputModel.IsReferencedBy.MapToResources(entity.IsReferencedBy).ToList();
        entity.Issued = inputModel.Issued;
        entity.Keyword = inputModel.Keywords.MapToKeywords(entity.Keyword).ToList();
        entity.LandingPage = inputModel.LandingPages.MapToResources(entity.LandingPage).ToList();
        entity.Language = inputModel.Languages.Select(x => x.Code).ToArray();
        entity.Modified = inputModel.Modified;
        entity.PreviousVersionId = inputModel.PreviousVersion?.Id;
        entity.ProcessId = inputModel.ProcessId;
        entity.PublisherId = publisherId;
        entity.QualifiedAttribution = inputModel.QualifiedAttributions
            .MapToQualifiedAttributions(entity.QualifiedAttribution, qualifiedAttributionsAgentsMappingTable)
            .ToList();
        entity.QualifiedAttributionComplement = inputModel.QualifiedAttributionComplement?.MapToMultiLanguage();
        entity.QualifiedRelation = inputModel.QualifiedRelations.MapToQualifiedRelations(entity.QualifiedRelation)
            .ToList();
        entity.Relation = inputModel.Relations.MapToResources(entity.Relation).ToList();
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.RetentionPeriod = inputModel.RetentionPeriod;
        entity.RetentionPeriodDescription = inputModel.RetentionPeriodComplement?.MapToMultiLanguage();
        entity.Spatial = inputModel.Spatial.ToArray();
        entity.TemporalCoverage = inputModel.TemporalCoverage.MapToPeriodsOfTime(entity.TemporalCoverage).ToList();
        entity.Title = inputModel.Title.MapToMultiLanguage();
        entity.Theme = inputModel.Themes.Select(x => x.Code).ToArray();
        entity.Version = inputModel.Version;
        entity.VersionNotes = inputModel.VersionNotes?.MapToMultiLanguage();

        return entity;
    }
}
