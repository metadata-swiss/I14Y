using Bfs.Iop.DataAccess.Abstractions;

namespace Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

public sealed record FilterDefinitionFilterValueModel
{
    public required MultiLanguageModel Name { get; init; } // the user selects this on the UI

    public required string FilterValueIdentifier { get; init; } // this is sent to backend

    public required List<FilterDefinitionValueModel> FieldValues { get; init; } // these are read from the content configuration

    public OccurType OccurType { get; init; }
}