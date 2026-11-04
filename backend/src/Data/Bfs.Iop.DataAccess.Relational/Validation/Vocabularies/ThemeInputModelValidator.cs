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

            // Naming a vocabulary narrows the search to it, so nothing is guessed from the code alone.
            // It need not be one of the standard theme taxonomies, only a vocabulary defined on I14Y.
            var allThemes = vocabulariesService.GetThemesFor(model);

            if (!string.IsNullOrWhiteSpace(model.VocabularyIdentifier) && allThemes.Count == 0)
            {
                context.AddFailure(nameof(model.VocabularyIdentifier), $"The vocabulary '{model.VocabularyIdentifier}' is not defined on I14Y, or none of its entries carries a URI.");
                return;
            }

            // Codes are only unique within a taxonomy, so a shared one needs the URI to settle it.
            var matchingCodes = hasCode
                ? allThemes.Where(x => x.Code == model.Code).DistinctBy(x => x.Uri, StringComparer.Ordinal).ToList()
                : [];

            if (hasCode && matchingCodes.Count == 0)
            {
                context.AddFailure(nameof(model.Code), $"The code '{model.Code}' does not resolve to any theme in {(string.IsNullOrWhiteSpace(model.VocabularyIdentifier) ? "a registered theme taxonomy" : $"'{model.VocabularyIdentifier}'")}, or its entry carries no URI (EXT_RESOURCE annotation).");
                return;
            }

            if (hasCode && !hasUri && matchingCodes.Count > 1)
            {
                context.AddFailure(nameof(model.Code), $"The code '{model.Code}' exists in several registered theme taxonomies ({string.Join(", ", matchingCodes.Select(x => x.Uri))}). Provide the 'uri' of the intended theme instead.");
                return;
            }

            var themeByUri = hasUri
                ? allThemes.FirstOrDefault(x => x.Uri == model.Uri)
                : null;

            if (hasUri && themeByUri is null)
            {
                context.AddFailure(nameof(model.Uri), $"The URI '{model.Uri}' does not resolve to any entry in {(string.IsNullOrWhiteSpace(model.VocabularyIdentifier) ? "a registered theme taxonomy" : $"'{model.VocabularyIdentifier}'")}.");
                return;
            }

            if (hasCode && hasUri && !matchingCodes.Any(x => x.Uri == themeByUri!.Uri))
            {
                context.AddFailure($"The code '{model.Code}' and the URI '{model.Uri}' do not refer to the same theme.");
            }
        });
    }
}
