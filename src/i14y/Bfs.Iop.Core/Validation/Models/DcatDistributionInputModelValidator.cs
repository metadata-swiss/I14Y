using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data.Contracts;
using Bfs.Iop.Core.Validation.Extensions;
using Bfs.Iop.Core.Validation.Vocabularies;
using Bfs.Iop.Core.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatDistributionInputModelValidator : AbstractValidator<DcatDistributionInputModel>
{
    public DcatDistributionInputModelValidator(
        VocabularyEntryCodeValidator<EuPlannedAvailabilityVocabulary> availabilityValidator,
        VocabularyEntryCodeValidator<FileTypesVocabulary> formatsValidator,
        VocabularyEntryCodeValidator<Iso639LanguagesVocabulary> languagesValidator,
        VocabularyEntryCodeValidator<LicenseTypesVocabulary> licenseValidator,
        VocabularyEntryCodeValidator<MediaTypesVocabulary> mediaTypesValidator,
        VocabularyEntryCodeValidator<PackingFormatsVocabulary> packagingFormatsValidator,
        IValidator<ResourceModel> resourceInputModelValidator,
        IValidator<ChecksumInputModel> checksumValidator,
        IValidator<PeriodOfTimeModel> periodOfTimeValidator,
        IDataServicesService dataServicesService
        )
    {
        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.AccessServices)
            .MustContainOnlyDistinctIds()
            .DependentRules(() =>
            {
                RuleForEach(x => x.AccessServices)
                .Must((_, x) =>
                {
                    var result = dataServicesService.GetUserAllowActionInfo(x.Id).GetAwaiter().GetResult();

                    return result.Single(x => x.ActionType is AllowActionType.Read).Value;
                })
                .WithMessage((_, x) => $"No suitable data service with the id '{x.Id}' has been found.");
            });

        RuleFor(x => x.AccessUrl)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.Availability!.Code)
            .SetValidator(availabilityValidator)
            .When(x => x.Availability is not null);

        RuleFor(x => x.ByteSize)
            .GreaterThanOrEqualTo(0.0m)
            .When(x => x.ByteSize.HasValue);

        RuleFor(x => x.Checksum)
            .SetValidator(checksumValidator!)
            .When(x => x.Checksum is not null);

        RuleForEach(x => x.ConformsTo)
            .SetValidator(resourceInputModelValidator);

        RuleForEach(x => x.Coverage)
            .SetValidator(periodOfTimeValidator);

        RuleFor(x => x.Description)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleForEach(x => x.Documentation)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.DownloadUrl)
            .Must((m, x) => x!.Uri == m.AccessUrl.Uri)
            .When(x => x.DownloadUrl is not null)
            .WithMessage(x => "Download url must be the same as the access url.")
            .DependentRules(() =>
            {
                RuleFor(x => x.DownloadUrl)
                .SetValidator(resourceInputModelValidator!)
                .When(x => x.DownloadUrl is not null);
            });

        RuleFor(x => x.Format!.Code)
            .SetValidator(formatsValidator)
            .When(x => x.Format is not null);

        RuleFor(x => x.Identifier!)
            .MustBeValidIdentifier()
            .When(x => x.Identifier is not null);

        RuleForEach(x => x.Images)
            .SetValidator(resourceInputModelValidator);

        RuleFor(x => x.Languages)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Languages.Select(x => x.Code))
                    .SetValidator(languagesValidator)
                    .WithName(x => nameof(x.Languages));
            });

        RuleFor(x => x.License!.Code)
            .SetValidator(licenseValidator)
            .When(x => x.License is not null);

        RuleFor(x => x.MediaType!.Code)
            .SetValidator(mediaTypesValidator)
            .When(x => x.MediaType is not null);

        RuleFor(x => x.PackagingFormat!.Code)
            .SetValidator(packagingFormatsValidator)
            .When(x => x.PackagingFormat is not null);

        RuleFor(x => x.SpatialResolution)
            .GreaterThanOrEqualTo(0m)
            .When(x => x.SpatialResolution.HasValue);

        RuleFor(x => x.Title)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();
    }
}
