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
        this IopConceptInputModel inputModel,
        Guid tenantId,
        Guid responsiblePersonId,
        Guid? responsibleDeputyId,
        Guid? publisherId,
        IIdentifierGenerator identifierGenerator,
        IVocabulariesService vocabulariesService,
        IopConcept? entity = null,
        IEnumerable<ResourceModel>? replaces = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.CodeListEntryValueType = inputModel.CodeListEntryValueType;
        entity.CodeListEntryValueMaxLength = inputModel.CodeListEntryValueMaxLength;
        entity.ConceptType = inputModel.ConceptType;
        entity.ConformsTo = inputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.CodeListEntryDefaultSortProperty = inputModel.CodeListEntryDefaultSortProperty;
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.ExternalPublisherIdentifier = publisherId is null ? inputModel.Publisher.Identifier : null;
        entity.ExternalPublisherName = publisherId is null ? inputModel.Publisher.Name?.MapToMultiLanguage() : null;
        entity.Identifiers = inputModel.Identifiers.Any()
            ? [.. inputModel.Identifiers]
            : [identifierGenerator.GenerateIdentifier<IopConcept>(inputModel.Name)];
        entity.Keywords = inputModel.Keywords.MapToKeywords(entity.Keywords).ToList();
        entity.MaxLength = inputModel.MaxLength;
        entity.MaxValue = inputModel.MaxValue;
        entity.MeasurementUnit = inputModel.MeasurementUnit;
        entity.MinLength = inputModel.MinLength;
        entity.MinValue = inputModel.MinValue;
        entity.Name = inputModel.Name.MapToMultiLanguage();
        entity.NumberDecimals = inputModel.NumberDecimals;
        entity.Pattern = inputModel.Pattern;
        entity.PublisherId = publisherId;
        entity.Replaces = (replaces ?? []).MapToResources(entity.Replaces).ToList();
        entity.ResponsibleDeputyId = responsibleDeputyId;
        entity.ResponsiblePersonId = responsiblePersonId;
        entity.TenantId = tenantId;
        entity.Themes = inputModel.Themes.Select(vocabulariesService.ResolveThemeInputToUri).Distinct().ToList();
        entity.ValidFrom = inputModel.ValidFrom;
        entity.ValidTo = inputModel.ValidTo;
        entity.Version = inputModel.Version;

        return entity;
    }
}