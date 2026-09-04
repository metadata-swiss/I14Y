using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class PublicServiceInputModelValidator : AbstractValidator<PublicServiceInputModel>
{
    private static Guid? _idToUpdate = null;

    public PublicServiceInputModelValidator(
        IValidator<ChannelInputModel> channelInputValidator,
        IValidator<KeywordModel> keywordValidator,
        VocabularyEntryCodeValidator<BkBusinessEventsVocabulary> businessEventsValidator,
        VocabularyEntryCodeValidator<Iso639LanguagesVocabulary> languagesValidator,
        VocabularyEntryCodeValidator<BkLifeEventsVocabulary> lifeEventsValidator,
        VocabularyEntryCodeValidator<ThemesVocabulary> themesValidator,
        VocabularyEntryCodeValidator<SpatialCHVocabulary> spatialCHValidator,
        IopDbContext dbContext)
    {
        RuleFor(_ => _)
           .Custom((model, context) =>
           {
               var idKey = ValidationContextDataKeys.IdKey;

               _idToUpdate = context.RootContextData.TryGetValue(idKey, out object? value)
                   ? (Guid)value
                   : null;
           });

        RuleFor(x => x.BusinessEvents)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.BusinessEvents.Select(x => x.Code))
                    .SetValidator(businessEventsValidator)
                    .WithName(x => nameof(x.BusinessEvents));
            });

        RuleFor(x => x.Channels)
            .Must((_, x) =>
            {
                var identifiers = x.Select(y => y.Identifier).ToArray();
                var distinct = identifiers.Distinct().ToArray();

                return identifiers.Length == distinct.Length;
            })
            .WithMessage("Channels identifiers must be unique.");

        RuleForEach(x => x.Channels)
            .SetValidator(channelInputValidator);

        RuleFor(x => x.Description)
            .NotNull()
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleForEach(x => x.Identifiers)
            .Must((x, identifier) => x.Identifiers.Count(s => s == identifier) == 1)
            .WithMessage((_, identifier) => $"The identifier '{identifier}' must be unique within the collection.");

        RuleForEach(x => x.Identifiers)
            .MustBeValidIdentifier();

        RuleForEach(x => x.Identifiers)
            .Must((_, x) => !dbContext.PublicServices.Any(y => y.Identifiers.Contains(x) && y.Id != _idToUpdate))
            .WithMessage((_, x) => $"A resource with the identifier '{x}' already exists");

        RuleFor(x => x.IsDescribedAt)
            .MustContainOnlyDistinctIds();

        RuleForEach(x => x.Keywords)
            .SetValidator(keywordValidator);

        RuleFor(x => x.Languages)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Languages.Select(x => x.Code))
                    .SetValidator(languagesValidator)
                    .WithName(x => nameof(x.Languages));
            });

        RuleFor(x => x.LifeEvents)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.LifeEvents.Select(x => x.Code))
                    .SetValidator(lifeEventsValidator)
                    .WithName(x => nameof(x.LifeEvents));
            });

        RuleFor(x => x.Name)
            .NotNull()
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleFor(x => x.Relations)
            .MustContainOnlyDistinctIds();

        RuleFor(x => x.Requires)
            .MustContainOnlyDistinctIds();

        RuleFor(_ => _)
            .Must(x => !string.Equals(x.ResponsibleDeputy!.Email, x.ResponsiblePerson!.Email, StringComparison.OrdinalIgnoreCase))
            .When(x => x.ResponsiblePerson is not null && x.ResponsibleDeputy is not null)
            .WithMessage(x => $"The {nameof(x.ResponsiblePerson)} and the {nameof(x.ResponsibleDeputy)} must be different.");

        RuleFor(x => x.Sectors)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.Sectors.Select(x => x.Code))
                    .SetValidator(themesValidator)
                    .WithName(x => nameof(x.Sectors));
            });

        RuleFor(x => x.ThematicAreas)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.ThematicAreas.Select(x => x.Code))
                    .SetValidator(themesValidator)
                    .WithName(x => nameof(x.ThematicAreas));
            });

        RuleFor(x => x.SpatialCH)
            .MustContainOnlyDistinctCodes()
            .DependentRules(() =>
            {
                RuleForEach(x => x.SpatialCH.Select(x => x.Code))
                    .SetValidator(spatialCHValidator)
                    .WithName(x => nameof(x.SpatialCH));
            });

        RuleForEach(x => x.Spatial)
            .NotEmpty();
    }
}
