using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Relational.Tools;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class IopConceptMappingExtensions
{
    public static IopConceptModel MapToIopConceptModel(
        this IopConcept entity,
        IVocabulariesService vocabulariesService,
        IEnumerable<ConceptReferenceModel>? replaces = null,
        IEnumerable<ConceptReferenceModel>? isReplacedBy = null)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        return new()
        {
            CodeListEntryValueMaxLength = entity.CodeListEntryValueMaxLength,
            CodeListEntryValueType = entity.CodeListEntryValueType,
            ConceptType = entity.ConceptType,
            ConformsTo = entity.ConformsTo.Select(r => r.MapToResourceModel()).ToList(),
            CodeListEntryDefaultSortProperty = entity.CodeListEntryDefaultSortProperty,
            Description = entity.Description.MapToMultiLanguageModel(),
            Id = entity.Id,
            Identifiers = entity.Identifiers,
            IsLocked = entity.IsLocked,
            Keywords = entity.Keywords.Select(k => k.MapToKeywordModel()).ToList(),
            MaxLength = entity.MaxLength,
            MaxValue = entity.MaxValue,
            MeasurementUnit = entity.MeasurementUnit,
            MinLength = entity.MinLength,
            MinValue = entity.MinValue,
            Name = entity.Name.MapToMultiLanguageModel(),
            NumberDecimals = entity.NumberDecimals,
            Pattern = entity.Pattern,
            Replaces = replaces?.ToList() ?? entity.Replaces.Select(r => new ConceptReferenceModel
            {
                Uri = r.Href,
                Name = r.Label?.MapToMultiLanguageModel()
            }).ToList(),
            IsReplacedBy = isReplacedBy?.ToList() ?? [],
            PublicationLevel = entity.PublicationLevel,
            PublicationLevelProposal = entity.PublicationLevelProposal,
            Publisher = entity.Publisher is not null
                ? entity.Publisher.MapToIdentifierNameModel()
                : new()
                {
                    Identifier = entity.ExternalPublisherIdentifier,
                    Name = entity.ExternalPublisherName?.MapToMultiLanguageModel()
                },
            RegistrationStatus = entity.RegistrationStatus,
            RegistrationStatusProposal = entity.RegistrationStatusProposal,
            ResponsibleDeputy = entity.ResponsibleDeputy?.MapToIopPersonModel(),
            ResponsiblePerson = entity.ResponsiblePerson?.MapToIopPersonModel(),
            System = entity.MapSystemInfoToSystemInfoModel(),
            Tenant = entity.Tenant.MapToAgentModel(vocabulariesService),
            Themes = vocabulariesService.ResolveThemeValues(entity.Themes),
            ValidFrom = entity.ValidFrom,
            ValidTo = entity.ValidTo,
            Version = entity.Version,
        };
    }

    public static IopConcept MapToIopConcept(
        this IopConceptInputModel iopConceptInputModel,
        Guid tenantId,
        Guid responsiblePersonId,
        Guid? responsibleDeputyId,
        IIdentifierGenerator identifierGenerator,
        IVocabulariesService vocabulariesService,
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
        entity.TenantId = tenantId;
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.Themes = iopConceptInputModel.Themes.Select(vocabulariesService.ResolveThemeInputToUri).Distinct().ToList();
        entity.ValidFrom = iopConceptInputModel.ValidFrom;
        entity.ValidTo = iopConceptInputModel.ValidTo;
        entity.Version = iopConceptInputModel.Version;

        return entity;
    }
}