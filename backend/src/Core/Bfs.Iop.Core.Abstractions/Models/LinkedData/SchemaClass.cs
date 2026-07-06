namespace Bfs.Iop.Core.Abstractions.Models.LinkedData;

public sealed record SchemaClass
{
    public MultiLanguageModel? Label { get; set; }

    public MultiLanguageModel? Description { get; set; }

    public required Uri UriComplete { get; set; }

    public string? TargetClass { get; set; }

    public SchemaPoint? Point { get; set; }

    public bool? Closed { get; set; }

    public string? Identifier { get; set; }

    public ICollection<SchemaProperty> Properties { get; init; } = [];
}

