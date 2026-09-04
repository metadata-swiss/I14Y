using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class ChannelInputModelValidator : AbstractValidator<ChannelInputModel>
{
    private static Guid? _publicServiceIdToUpdate = null;

    public ChannelInputModelValidator(
        VocabularyEntryCodeValidator<ChannelTypesVocabulary> channelTypesValidator,
        IopDbContext dbContext)
    {
        RuleFor(_ => _)
           .Custom((model, context) =>
           {
               var idKey = ValidationContextDataKeys.IdKey;

               _publicServiceIdToUpdate = context.RootContextData.TryGetValue(idKey, out object? value)
                   ? (Guid)value
                   : null;
           });

        RuleFor(x => x.Identifier)
            .MustBeValidIdentifier();

        // Identifier must be unique in the database
        RuleFor(x => x.Identifier) 
            .Must(x => !dbContext.Channels.Any(y => y.Identifier == x && y.PublicServiceId != _publicServiceIdToUpdate))
            .WithMessage((_, x) => $"One channel with the identifier '{x}' already exists.");

        RuleFor(x => x.OwnedBy)
            .MustContainOnlyDistinctIdentifiers();

        RuleFor(x => x.Type!.Code)
            .SetValidator(channelTypesValidator)
            .When(x => x.Type is not null)
            .DependentRules(() =>
            {
                When(x => x.Type is not null, () =>
                {
                    SetRulesForFaxType();
                    SetRulesForEmailType();
                    SetRulesForMobileType();
                    SetRulesForPhoneType();
                    SetRulesForPostType();
                    SetRulesForWebType();
                });
            });
    }

    private void SetRulesForPostType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.PostCode,
            () => RuleFor(x => x.Address)
                    .NotNull()
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Address!)
                            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();
                    }));

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.PostCode,
            () => RuleFor(x => x.Address)
                .Null()
                .WithMessage("Value must be null."));
    }

    private void SetRulesForEmailType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.EmailCode,
            () => RuleFor(x => x.Email)
                    .NotEmpty()
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Email)
                            .Must(email => email!.IsValidEmail())
                            .WithMessage("The value is not a valid email.");
                    }));

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.EmailCode,
            () => RuleFor(x => x.Email).Null());
    }

    private void SetRulesForMobileType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.MobilePhoneCode,
            () => RuleFor(x => x.Mobile)
                    .NotEmpty());

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.MobilePhoneCode,
            () => RuleFor(x => x.Mobile)
                    .Null()
                    .WithMessage("Value must be null."));
    }

    private void SetRulesForFaxType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.FaxCode,
            () => RuleFor(x => x.Fax)
                    .NotEmpty());

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.FaxCode,
            () => RuleFor(x => x.Fax)
                    .Null()
                    .WithMessage("Value must be null."));
    }

    private void SetRulesForWebType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.WebCode,
            () => RuleFor(x => x.Url)
                    .NotEmpty()
                    .DependentRules(() =>
                    {
                        RuleFor(x => x.Url)
                            .Must(uri => uri!.IsValidUri())
                            .WithMessage("The value is not a valid Url.");
                    }));

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.WebCode,
            () => RuleFor(x => x.Url)
                    .Null()
                    .WithMessage("Value must be null."));
    }

    private void SetRulesForPhoneType()
    {
        When(x => x.Type!.Code == ChannelTypesVocabulary.Codes.PhoneCode,
            () => RuleFor(x => x.Phone)
                    .NotEmpty());

        When(x => x.Type!.Code != ChannelTypesVocabulary.Codes.PhoneCode,
            () => RuleFor(x => x.Phone)
                    .Null()
                    .WithMessage("Value must be null."));
    }
}
