using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Tools;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class IopConceptMappingExtensions
{
    public static IopConceptModel MapToIopConceptModel(
        this IopConcept iopConcept,
        IVocabulariesService vocabulariesService,
        IEnumerable<ConceptReferenceModel>? replaces = null,
        IEnumerable<ConceptReferenceModel>? isReplacedBy = null)
    {
        ArgumentNullException.ThrowIfNull(iopConcept, nameof(iopConcept));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var themesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ThemesVocabulary>();

        return new()
        {
            CodeListEntries = iopConcept.CodeListEntries?.Select(c => c.MapToCodeListEntryModel()).ToList(),
            CodeListEntryValueMaxLength = iopConcept.CodeListEntryValueMaxLength,
            CodeListEntryValueType = iopConcept.CodeListEntryValueType,
            ConceptType = iopConcept.ConceptType,
            ConformsTo = iopConcept.ConformsTo.Select(r => r.MapToResourceModel()).ToList(),
            CodeListEntryDefaultSortProperty = iopConcept.CodeListEntryDefaultSortProperty,
            Description = iopConcept.Description.MapToMultiLanguageModel(),
            Id = iopConcept.Id,
            Identifiers = iopConcept.Identifiers,
            IsLocked = iopConcept.IsLocked,
            Keywords = iopConcept.Keywords.Select(k => k.MapToKeywordModel()).ToList(),
            MaxLength = iopConcept.MaxLength,
            MaxValue = iopConcept.MaxValue,
            MeasurementUnit = iopConcept.MeasurementUnit,
            MinLength = iopConcept.MinLength,
            MinValue = iopConcept.MinValue,
            Name = iopConcept.Name.MapToMultiLanguageModel(),
            NumberDecimals = iopConcept.NumberDecimals,
            Pattern = iopConcept.Pattern,
            Replaces = replaces?.ToList() ?? iopConcept.Replaces.Select(r => new ConceptReferenceModel
            {
                Uri = r.Href,
                Name = r.Label?.MapToMultiLanguageModel()
            }).ToList(),
            IsReplacedBy = isReplacedBy?.ToList() ?? [],
            PublicationLevel = iopConcept.PublicationLevel,
            PublicationLevelProposal = iopConcept.PublicationLevelProposal,
            Publisher = iopConcept.Publisher.MapToAgentModel(vocabulariesService),
            RegistrationStatus = iopConcept.RegistrationStatus,
            RegistrationStatusProposal = iopConcept.RegistrationStatusProposal,
            ResponsibleDeputy = iopConcept.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = iopConcept.ResponsiblePerson?.MapToIopPersonModel(),
            System = iopConcept.MapSystemInfoToSystemInfoModel(),
            Themes = themesVocabulary != null ? iopConcept.Themes.MapToVocabularyEntryModels(themesVocabulary).ToList() : [],
            ValidFrom = iopConcept.ValidFrom,
            ValidTo = iopConcept.ValidTo,
            Version = iopConcept.Version,
        };
    }

    public static IopConcept MapToIopConcept(
        this IopConceptInputModel iopConceptInputModel,
        Guid publisherId,
        Guid responsiblePersonId,
        Guid? responsibleDeputyId,
        IIdentifierGenerator identifierGenerator,
        IopConcept? entity = null,
        IEnumerable<ResourceModel>? replaces = null)
    {
        ArgumentNullException.ThrowIfNull(iopConceptInputModel, nameof(iopConceptInputModel));

        entity ??= new();

        entity.CodeListEntryValueType = iopConceptInputModel.CodeListEntryValueType;
        entity.CodeListEntryValueMaxLength = iopConceptInputModel.CodeListEntryValueMaxLength;
        entity.ConceptType = iopConceptInputModel.ConceptType;
        entity.ConformsTo = iopConceptInputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.CodeListEntryDefaultSortProperty = iopConceptInputModel.CodeListEntryDefaultSortProperty;
        entity.Description = iopConceptInputModel.Description.MapToMultiLanguage();
        entity.Identifiers = iopConceptInputModel.Identifiers.Any()
            ? [.. iopConceptInputModel.Identifiers]
            : [identifierGenerator.GenerateIdentifier<IopConcept>(iopConceptInputModel.Name)];
        entity.Keywords = iopConceptInputModel.Keywords.MapToKeywords(entity.Keywords).ToList();
        entity.MaxLength = iopConceptInputModel.MaxLength;
        entity.MaxValue = iopConceptInputModel.MaxValue;
        entity.MeasurementUnit = iopConceptInputModel.MeasurementUnit;
        entity.MinLength = iopConceptInputModel.MinLength;
        entity.MinValue = iopConceptInputModel.MinValue;
        entity.Name = iopConceptInputModel.Name.MapToMultiLanguage();
        entity.NumberDecimals = iopConceptInputModel.NumberDecimals;
        entity.Pattern = iopConceptInputModel.Pattern;
        entity.Replaces = (replaces ?? []).MapToResources(entity.Replaces).ToList();
        entity.PublisherId = publisherId;
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.Themes = iopConceptInputModel.Themes.Select(x => x.Code).ToList();
        entity.ValidFrom = iopConceptInputModel.ValidFrom;
        entity.ValidTo = iopConceptInputModel.ValidTo;
        entity.Version = iopConceptInputModel.Version;

        return entity;
    }
}