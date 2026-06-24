using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class ResourceInputModelValidator : AbstractValidator<ResourceModel>
{
    public ResourceInputModelValidator()
    {
        ClassLevelCascadeMode = CascadeMode.Stop;

        RuleFor(x => x.Uri)
            .Must(uri => uri.IsValidUri())
            .WithMessage((_, uri) => $"'{uri}' is not a valid uri.");
    }
}
