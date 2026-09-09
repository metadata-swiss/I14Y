using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Entities;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;

namespace Bfs.Iop.DataAccess.Relational.Mappings;

internal static class DistributionMappingExtensions
{
    public static DcatDistributionModel MapToDcatDistributionModel(
        this Distribution entity,
        IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        var euPlannedAvailabilityVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<EuPlannedAvailabilityVocabulary>();
        var checksumAlgorithmsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<ChecksumAlgorithmsVocabulary>();
        var fileTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<FileTypesVocabulary>();
        var iso639LanguagesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<Iso639LanguagesVocabulary>();
        var licenseTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<LicenseTypesVocabulary>();
        var mediaTypesVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<MediaTypesVocabulary>();
        var packingFormatsVocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<PackingFormatsVocabulary>();

        return new()
        {
            AccessServices = entity.AccessServices.Select(x => x.MapToIdModel()).ToList(),
            AccessUrl = entity.AccessUrl.FirstOrDefault()?.MapToResourceModel(),
            Availability = entity.AvailabilityVocabulary?.MapToVocabularyEntryModel(euPlannedAvailabilityVocabulary),
            ByteSize = entity.ByteSize,
            Checksum = entity.Checksum?.MapToChecksumModel(checksumAlgorithmsVocabulary),
            ConformsTo = entity.ConformsTo.Select(x => x.MapToResourceModel()).ToList(),
            Coverage = entity.Coverage.Select(x => x.MapToDateOnlyPeriodOfTimeModel()).ToList(),
            Description = entity.Description.MapToMultiLanguageModel(),
            Documentation = entity.Documentation.Select(x => x.MapToResourceModel()).ToList(),
            DownloadUrl = entity.DownloadUrl.FirstOrDefault()?.MapToResourceModel(),
            Format = entity.Format?.MapToVocabularyEntryModel(fileTypesVocabulary),
            Id = entity.Id,
            Identifier = entity.Identifier,
            Images = entity.Image.Select(x => x.MapToResourceModel()).ToList(),
            Issued = entity.Issued,
            Languages = entity.Language?.MapToVocabularyEntryModels(iso639LanguagesVocabulary).ToList() ?? [],
            License = entity.License?.MapToVocabularyEntryModel(licenseTypesVocabulary),
            MediaType = entity.MediaType?.MapToVocabularyEntryModel(mediaTypesVocabulary),
            Modified = entity.Modified,
            PackagingFormat = entity.PackagingFormat?.MapToVocabularyEntryModel(packingFormatsVocabulary),
            Rights = entity.Rights,
            SpatialResolution = entity.SpatialResolution is not null && 
                entity.SpatialResolution.Length == 1 && 
                decimal.TryParse(entity.SpatialResolution.First(), out _)
                    ? decimal.Parse(entity.SpatialResolution.First())
                    : null,
            TemporalResolution = entity.TemporalResolution,
            Title = entity.Title.MapToMultiLanguageModel()
        };
    }

    public static Distribution MapToDistribution(
        this DcatDistributionInputModel inputModel, 
        Distribution? entity = null)
    {
        ArgumentNullException.ThrowIfNull(inputModel, nameof(inputModel));

        entity ??= new();

        entity.AccessServices = inputModel.AccessServices.MapToDistributionDataServiceRelations(entity.AccessServices).ToList();
        entity.AccessUrl = new[] { inputModel.AccessUrl }.MapToResources(entity.AccessUrl).ToList();
        entity.AvailabilityVocabulary = inputModel.Availability?.Code;
        entity.ByteSize = inputModel.ByteSize;
        entity.Checksum = inputModel.Checksum?.MapToCheckSum(entity.Checksum);
        entity.ConformsTo = inputModel.ConformsTo.MapToResources(entity.ConformsTo).ToList();
        entity.Coverage = inputModel.Coverage.MapToPeriodsOfTime(entity.Coverage).ToList();
        entity.Description = inputModel.Description.MapToMultiLanguage();
        entity.Documentation = inputModel.Documentation.MapToResources(entity.Documentation).ToList();
        entity.DownloadUrl = inputModel.DownloadUrl is not null
            ? new[] { inputModel.DownloadUrl }.MapToResources(entity.DownloadUrl).ToList()
            : [];
        entity.Format = inputModel.Format?.Code;
        entity.Id = inputModel.Id ?? default;
        entity.Identifier = inputModel.Identifier;
        entity.Image = inputModel.Images.MapToResources(entity.Image).ToList();
        entity.Issued = inputModel.Issued;
        entity.Language = inputModel.Languages.Select(l => l.Code).ToArray();
        entity.License = inputModel.License?.Code;
        entity.Modified = inputModel.Modified;
        entity.MediaType = inputModel.MediaType?.Code;
        entity.PackagingFormat = inputModel.PackagingFormat?.Code;
        entity.Rights = inputModel.Rights;
        entity.SpatialResolution = inputModel.SpatialResolution.HasValue 
            ? [inputModel.SpatialResolution.Value.ToString()] 
            : [];
        entity.TemporalResolution = inputModel.TemporalResolution;
        entity.Title = inputModel.Title.MapToMultiLanguage();

        return entity;
    }

    public static IEnumerable<Distribution> MapToDistributions(
        this IEnumerable<DcatDistributionInputModel> inputModels,
        ICollection<Distribution> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            d => d.MapToDistribution(entities.SingleOrDefault(x => x.Id == d.Id) ?? new()));
    }

    private static IdModel MapToIdModel(this DistributionDataServiceRelation entity)
    {
        ArgumentNullException.ThrowIfNull(entity, nameof(entity));

        return new()
        {
            Id = entity.DataServiceId
        };
    }

    private static DistributionDataServiceRelation MapToDistributionDataServiceRelation(
        this IdModel idModel,
        DistributionDataServiceRelation? entity = null)
    {
        ArgumentNullException.ThrowIfNull(idModel, nameof(idModel));

        entity ??= new();

        entity.DataServiceId = idModel.Id;

        return entity;
    }

    private static IEnumerable<DistributionDataServiceRelation> MapToDistributionDataServiceRelations(
        this IEnumerable<IdModel> inputModels,
        ICollection<DistributionDataServiceRelation> entities)
    {
        ArgumentNullException.ThrowIfNull(inputModels, nameof(inputModels));
        ArgumentNullException.ThrowIfNull(entities, nameof(entities));

        return inputModels.Select(
            (r, i) => r.MapToDistributionDataServiceRelation(entities.Count > i
                ? entities.ElementAt(i)
                : new()));
    }
}
