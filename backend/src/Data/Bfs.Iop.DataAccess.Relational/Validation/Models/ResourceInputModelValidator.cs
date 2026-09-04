using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Validation.Extensions;
using FluentValidation;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

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
