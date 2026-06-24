namespace Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

public sealed record FilterDefinitionModel
{
    public required string FilterIdentifier { get; init; }

    public required MultiLanguageModel Name { get; init; }

    public MultiLanguageModel? Description { get; init; }

    public List<FilterDefinitionFilterValueModel> Values { get; init; } = [];
}