using Bfs.Iop.Core.Abstractions.Models;
using Bfs.Iop.Core.Data;
using Bfs.Iop.Core.Data.Entities;
using Bfs.Iop.Core.Services.Contracts;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.Core.Validation.Models;

internal sealed class DcatCatalogRecordInputModelValidator : AbstractValidator<DcatCatalogRecordInputModel>
{
    public DcatCatalogRecordInputModelValidator(
        IopDbContext dbContext, 
        IVocabulariesService vocabulariesService)
    {
        RuleFor(x => x)
            .Custom((model, context) =>
            {
                var dcatCatalog = (DcatCatalog)context.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey];
                var catalogRecordToUpdateId = context.RootContextData.TryGetValue(ValidationContextDataKeys.IdKey, out object? value) 
                    ? (Guid?)value 
                    : null;

                // Validate if a record with the same resource already exists in the catalog
                var exists = dbContext.DcatCatalogRecords
                    .Include(x => x.PrimaryTopic)
                    .Where(x =>
                        x.DcatCatalogId == dcatCatalog.Id &&
                        x.PrimaryTopic.ResourceId == model.PrimaryTopic.ResourceId &&
                        x.Id != catalogRecordToUpdateId)
                    .Any();

                if (exists)
                {
                    context.AddFailure(nameof(model), $"A catalog record for the resource with the id '{model.PrimaryTopic.ResourceId}' already exists in the catalog.");
                }

                // Validate the primary topic
                var primaryTopic = tryFindPrimaryTopic(model.PrimaryTopic);

                if (primaryTopic is null)
                {
                    context.AddFailure(nameof(model.PrimaryTopic), "No resource has been found.");
                }
                else if (primaryTopic.PublisherId != dcatCatalog.Publisher.Id)
                {
                    context.AddFailure(nameof(model.PrimaryTopic), "The resource and the dcat catalog must have the same publisher.");
                }

                PublishableEntityBase? tryFindPrimaryTopic(DcatCatalogResourceModel primaryTopic)
                {
                    return primaryTopic.ResourceType switch
                    {
                        DcatCatalogType.Dataset => dbContext.Find<Dataset>(primaryTopic.ResourceId),
                        DcatCatalogType.DataService => dbContext.Find<DataService>(primaryTopic.ResourceId),
                        _ => null
                    };
                }
            });

        RuleFor(model => model.Themes)
            .Must(codes => codes.Select(x => $"{x.ThemeTaxonomy.ToLowerInvariant()}.{x.Code.ToLowerInvariant()}").Distinct().Count() == codes.Count())
            .WithMessage(_ => "The collection cannot contain repeated codes.")
            .DependentRules(() =>
            {
                RuleForEach(model => model.Themes)
                    .Must((_, item, ctx) =>
                    {
                        var dcatCatalog = (DcatCatalog)ctx.RootContextData[ValidationContextDataKeys.DcatCatalogEntityKey];

                        return dcatCatalog.ThemeTaxonomy.Contains(item.ThemeTaxonomy);
                    })
                    .WithMessage((_, item) => $"The taxonomy '{item.ThemeTaxonomy}' is not defined in the catalog.")
                    .Must(item =>
                    {
                        var vocabulary = vocabulariesService.TryGetVocabulary(item.ThemeTaxonomy, default).GetAwaiter().GetResult();

                        return vocabulary?.Entries.Any(e => e.Code == item.Code) ?? false; // ToDo: This should be True when we are able to accept themes from external sources
                    })
                    .WithMessage((_, item) => $"The code '{item.Code}' does not exist in vocabulary '{item.ThemeTaxonomy}'.");
            });
    }
}
