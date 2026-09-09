namespace Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

public sealed record FilterConfigurationModel
{    
    public required List<FilterDefinitionModel> Filters { get; init; }
}