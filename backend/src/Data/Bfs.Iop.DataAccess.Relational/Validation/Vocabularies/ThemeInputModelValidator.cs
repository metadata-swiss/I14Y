using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;

internal sealed class ThemeInputModelValidator : AbstractValidator<ThemeInputModel>
{
    public ThemeInputModelValidator(IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        RuleFor(x => x).Custom((model, context) =>
        {
            var hasCode = !string.IsNullOrWhiteSpace(model.Code);
            var hasUri = !string.IsNullOrWhiteSpace(model.Uri);

            if (!hasCode && !hasUri)
            {
                context.AddFailure("Either 'code' or 'uri' must be provided.");
                return;
            }

            if (hasUri && !Uri.IsWellFormedUriString(model.Uri, UriKind.Absolute))
            {
                context.AddFailure(nameof(model.Uri), $"The URI '{model.Uri}' is not a well-formed absolute URI.");
                return;
            }

            var allThemes = vocabulariesService.GetThemes();

            var themeByCode = hasCode
                ? allThemes.FirstOrDefault(x => x.Code == model.Code)
                : null;

            if (hasCode && themeByCode is null)
            {
                context.AddFailure(nameof(model.Code), $"The code '{model.Code}' does not resolve to any theme: it belongs to no registered theme taxonomy, or its entry carries no URI (EXT_RESOURCE annotation).");
                return;
            }

            var themeByUri = hasUri
                ? allThemes.FirstOrDefault(x => x.Uri == model.Uri)
                : null;

            if (hasUri && themeByUri is null)
            {
                context.AddFailure(nameof(model.Uri), $"The URI '{model.Uri}' does not resolve to any entry in a registered theme taxonomy.");
                return;
            }

            if (hasCode && hasUri && themeByCode!.Uri != themeByUri!.Uri)
            {
                context.AddFailure($"The code '{model.Code}' and the URI '{model.Uri}' do not refer to the same theme.");
            }
        });
    }
}
