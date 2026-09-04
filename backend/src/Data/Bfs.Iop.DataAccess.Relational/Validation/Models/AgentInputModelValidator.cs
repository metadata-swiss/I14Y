using Bfs.Iop.Common.Options;
using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;
using Microsoft.Extensions.Options;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class AgentInputModelValidator : AbstractValidator<AgentInputModel>
{
    public AgentInputModelValidator(
        VocabularyEntryCodeValidator<SpatialCHVocabulary> spatialCHValidator,
        VocabularyEntryCodeValidator<LegalFormVocabulary> legalFormValidator,
        IValidator<ResourceModel> resourcesValidator,
        IOptions<I14YOptions> options)
    {
        RuleFor(x => x.Classification!.Code)
            .SetValidator(legalFormValidator)
            .When(x => x.Classification is not null);

        RuleFor(x => x.HomePage)
            .Must(str => str!.IsValidUri())
            .When(x => x.HomePage is not null)
            .WithMessage("The value must be a valid Url.");

        RuleFor(x => x.Name)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleFor(x => x.PrefLabel)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleFor(x => x.Identifier)
            .MustBeValidIdentifier();

        RuleForEach(x => x.Images)
            .SetValidator(resourcesValidator);

        var mediaBaseUrl = (options.Value.MediaBaseUrl ?? string.Empty).TrimEnd('/');

        RuleForEach(x => x.Images)
            .Must(x => !string.IsNullOrWhiteSpace(mediaBaseUrl)
                && x.Uri.StartsWith(mediaBaseUrl, StringComparison.OrdinalIgnoreCase))
            .WithMessage(_ => $"The image must be hosted in '{mediaBaseUrl}'.");

        RuleForEach(x => x.Spatial)
            .NotEmpty();

        RuleFor(x => x.SpatialCH)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.SpatialCH.Select(x => x.Code))
                    .SetValidator(spatialCHValidator)
                    .WithName(x => nameof(x.SpatialCH));
            });

        RuleFor(x => x.Uid)
            .Must(str => str!.IsValidUid())
            .When(x => x.Uid is not null)
            .WithMessage("The value must be a valid Uid.");
    }
}
