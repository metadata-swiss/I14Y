using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatCatalogInputModelValidator : AbstractValidator<DcatCatalogInputModel>
{
    public DcatCatalogInputModelValidator()
    {
        RuleFor(x => x.Description)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();

        RuleFor(x => x.ThemeTaxonomy)
            .Must(x => x.Distinct().Count() == x.Count())
            .WithMessage(_ => "The collection cannot contain repeated items.");

        RuleFor(x => x.Publisher.Identifier)
            .MustBeValidIdentifier();

        RuleFor(x => x.Title)
            .MustHaveAtLeastOneLanguageNotNullEmptyOrWhiteSpace();
    }
}
