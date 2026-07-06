namespace Bfs.Iop.Core.Abstractions.Models.FilterConfigurations;

public sealed record FilterDefinitionValueModel
{
    public required string FieldName { get; init; } // type

    public string? FieldValue { get; init; } // the value whats supposed to be in the FieldProperty if set to NULL then only the FieldName is considered

    public MultiLanguageModel? FieldValueMultilanguage { get; init; } // used then FieldProperty is set to text

    public string? FieldProperty { get; init; }  // usually title or text
}