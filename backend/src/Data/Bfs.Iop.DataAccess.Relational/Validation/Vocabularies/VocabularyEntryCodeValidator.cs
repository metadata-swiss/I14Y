using Bfs.Iop.DataAccess.Contracts;
using Bfs.Iop.DataAccess.Relational.Extensions;
using Bfs.Iop.DataAccess.Vocabularies;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Vocabularies;

internal class VocabularyEntryCodeValidator<T> : AbstractValidator<string> where T : IdentifiedVocabularyBase, new()
{
    public VocabularyEntryCodeValidator(IVocabulariesService vocabulariesService)
    {
        ArgumentNullException.ThrowIfNull(vocabulariesService, nameof(vocabulariesService));

        RuleLevelCascadeMode = CascadeMode.Stop;

        RuleFor(_ => _)
            .NotEmpty()
            .Must(x =>
            {
                var vocabulary = vocabulariesService.GetExistingOrEmptyVocabulary<T>();
                return vocabulary.Entries.Any(e => e.Code == x);
            })
            .WithMessage(x => $"Code '{x}' is not available in vocabulary '{new T().Identifier}'.");
    }
}
