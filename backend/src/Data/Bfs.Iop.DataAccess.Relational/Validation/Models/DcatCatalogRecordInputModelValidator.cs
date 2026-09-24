using Bfs.Iop.DataAccess.Abstractions;
using Bfs.Iop.DataAccess.Relational.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;

namespace Bfs.Iop.DataAccess.Relational.Validation.Models;

internal sealed class DcatCatalogRecordInputModelValidator : AbstractValidator<DcatCatalogRecordInputModel>
{
    public DcatCatalogRecordInputModelValidator(IopDbContext dbContext)
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
    }
}
